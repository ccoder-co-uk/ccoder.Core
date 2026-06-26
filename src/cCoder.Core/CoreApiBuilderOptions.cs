using cCoder.AppSecurity;
using cCoder.AppSecurity.Models;
using cCoder.ContentManagement;
using cCoder.ContentManagement.Models;
using cCoder.Core.Models;
using cCoder.Data;
using cCoder.DocumentManagement;
using cCoder.DocumentManagement.Models;
using cCoder.Logging;
using cCoder.Logging.Models;
using cCoder.Mail;
using cCoder.Mail.Models;
using cCoder.Scheduling;
using cCoder.Scheduling.Models;
using cCoder.Security;
using cCoder.Security.Api;
using cCoder.Security.Data.EF.MSSQL;
using cCoder.Security.Exposures;
using cCoder.Security.Objects;
using cCoder.Core.Exposures;
using cCoder.Core.Services.Foundations.Eventing;
using cCoder.Workflow;
using cCoder.Workflow.Models;
using cCoder.Eventing.Models;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.Http;
using cCoder.Eventing.Http.Models;
using Microsoft.OData.ModelBuilder;
using System.Text.Json.Serialization;
using ContentManagementRuntimeConfig = cCoder.ContentManagement.Models.Config;
using MailRuntimeConfig = cCoder.Mail.Models.Config;

namespace cCoder.Core;

public partial class CoreApiBuilderOptions
{
    private readonly Dictionary<string, List<Action<ODataConventionModelBuilder>>> routeContributors =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly List<EventProvider> eventProviders = [];

    private readonly IServiceCollection services;
    private CoreConfiguration coreConfiguration;
    private string sessionCacheConnectionString;
    private bool applied;

    public CoreApiBuilderOptions(IServiceCollection services) =>
        this.services = services;

    public CoreApiBuilderOptions WithCoreConfiguration(Action<CoreConfiguration> configure)
    {
        coreConfiguration ??= new CoreConfiguration();
        configure?.Invoke(coreConfiguration);

        Data.Config runtimeConfiguration = CreateRuntimeConfiguration(coreConfiguration);
        services.AddSingleton(coreConfiguration);
        services.AddSingleton(runtimeConfiguration);
        services.AddSingleton(CreateContentManagementRuntimeConfig(runtimeConfiguration));
        services.AddSingleton(CreateMailRuntimeConfig(runtimeConfiguration));

        return this;
    }

    public CoreApiBuilderOptions WithCoreConfiguration(Data.Config configuration)
    {
        configuration ??= new Data.Config();

        return WithCoreConfiguration(coreConfig =>
            CoreConfigurationMapper.PopulateFromRuntimeConfiguration(coreConfig, configuration));
    }

    public CoreApiBuilderOptions WithEventProviders(params EventProvider[] eventProviders)
    {
        this.eventProviders.AddRange((eventProviders ?? []).Where(provider => provider is not null));
        return this;
    }

    public CoreApiBuilderOptions AddStorage(string connectionString = null)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
            EnsureCoreConfiguration().CoreConnectionString = connectionString;

        return this;
    }

    public CoreApiBuilderOptions WithSessionCache(string connectionString)
    {
        sessionCacheConnectionString = connectionString;
        return this;
    }

    public CoreApiBuilderOptions WithSecurity(
        string connectionString,
        string decryptionKey,
        string rootPath = "Api/Security") =>
        WithCoreConfiguration(coreConfig =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
                coreConfig.SecurityConnectionString = connectionString;

            if (!string.IsNullOrWhiteSpace(decryptionKey))
                coreConfig.DecryptionKey = decryptionKey;

            if (!string.IsNullOrWhiteSpace(rootPath))
                coreConfig.SecurityRootPath = rootPath;
        });

    public CoreApiBuilderOptions UseHttpEventing(
        string hubUrl,
        Action<HttpEventingOptions> configure = null) =>
        WithCoreConfiguration(coreConfig =>
        {
            coreConfig.EnableHttpEventing = true;

            if (!string.IsNullOrWhiteSpace(hubUrl))
                coreConfig.HttpEventHubUrl = hubUrl;

            if (configure is not null)
            {
                HttpEventingOptions eventingOptions = new();
                configure(eventingOptions);
                coreConfig.MaxConcurrency = eventingOptions.MaxConcurrency;
            }
        });

    public CoreApiBuilderOptions UseServiceBusEventing(string connectionString) =>
        WithCoreConfiguration(coreConfig =>
        {
            coreConfig.EnableServiceBusEventing = true;

            if (!string.IsNullOrWhiteSpace(connectionString))
                coreConfig.ServiceBusConnectionString = connectionString;
        });

    public CoreApiBuilderOptions AddSecurityApi(
        Action<IServiceCollection, SecurityConfiguration> configure = null)
    {
        string rootPath = "Api/Security";

        cCoder.Security.IServiceCollectionExtensions.AddSecurityApi(services, (securityServices, securityConfig) =>
        {
            securityConfig.RootPath = coreConfiguration?.SecurityRootPath ?? rootPath;
            securityConfig.AddMSSQLModelProvider(
                securityServices,
                coreConfiguration?.SecurityConnectionString ?? string.Empty);
            securityConfig.UseAESHMMACPasswordEncryption(
                securityServices,
                coreConfiguration?.DecryptionKey ?? string.Empty);
            configure?.Invoke(securityServices, securityConfig);
            rootPath = EnsureRoutePath(securityConfig.RootPath, "Security");
            securityConfig.RootPath = null;
        });

        RegisterContext(rootPath, static builder => builder.ConfigureCoreSecurityApiModel());

        return this;
    }

    public CoreApiBuilderOptions AddAllDomains(string connection) =>
        AddAllDomains(domains => domains.Connection = connection);

    public CoreApiBuilderOptions AddAllDomains(Action<CoreDomainsConfig> configure)
    {
        CoreDomainsConfig domains = new();
        configure(domains);

        if (string.IsNullOrWhiteSpace(domains.Connection)
            && !string.IsNullOrWhiteSpace(coreConfiguration?.CoreConnectionString))
        {
            domains.Connection = coreConfiguration.CoreConnectionString;
        }

        if (string.IsNullOrWhiteSpace(domains.Connection))
        {
            throw new InvalidOperationException(
                "CoreDomainsConfig.Connection must be provided when adding the core business domains or available via core configuration.");
        }

        cCoder.Data.IServiceCollectionExtensions.AddCoreData(services, domains.Connection);

        AddAppSecurityApi(domain => ConfigureDomainRouting(domain, "AppSecurity", domains));
        AddContentManagementApi(domain => ConfigureDomainRouting(domain, "ContentManagement", domains));
        AddDocumentManagementApi(domain => ConfigureDomainRouting(domain, "DocumentManagement", domains));
        AddLoggingApi(domain => ConfigureDomainRouting(domain, "Logging", domains));
        AddMailApi(domain => ConfigureDomainRouting(domain, "Mail", domains));
        AddSchedulingApi(domain => ConfigureDomainRouting(domain, "Scheduling", domains));
        AddWorkflowApi(domain => ConfigureDomainRouting(domain, "Workflow", domains));

        return this;
    }

    public CoreApiBuilderOptions UseLegacyCoreApi(string routePath = "Api/Core")
    {
        RegisterContext(routePath, static builder => builder.ConfigureCoreAggregateApiModel());
        return this;
    }

    public CoreApiBuilderOptions UseLegacyCoreContext(string routePath = "Api/Core") =>
        UseLegacyCoreApi(routePath);

    public CoreApiBuilderOptions ConfigureDomainsWith(Action<CoreConfiguration> configure)
    {
        CoreConfiguration configuration = new();
        configure?.Invoke(configuration);

        WithCoreConfiguration(coreConfig =>
            CoreConfigurationMapper.Copy(configuration, coreConfig));

        AddStorage(configuration.CoreConnectionString);
        WithSecurity(
            configuration.SecurityConnectionString,
            configuration.DecryptionKey,
            configuration.SecurityRootPath);
        AddAppSecurityApi();
        AddContentManagementApi();
        AddDocumentManagementApi();
        AddLoggingApi();
        AddMailApi();
        AddSchedulingApi();
        AddWorkflowApi();
        UseLegacyCoreApi();
        UseConfiguredExternalEventing(configuration);
        WithEventProviders(configuration.EventProviders ?? []);

        return this;
    }

    public CoreApiBuilderOptions UseAll(Action<CoreConfiguration> configure) =>
        ConfigureDomainsWith(configure);

    public CoreApiBuilderOptions AddAppSecurityApi(
        Action<AppSecurityConfiguration> configure = null)
    {
        AppSecurityConfiguration domain = new();
        ApplyCoreDefaults(domain);
        configure?.Invoke(domain);

        services.AddAppSecurityWeb(
            configuration =>
            {
                ApplyConfiguration(domain, configuration);
                configuration.IncludeLegacyCoreContext = false;
            },
            new ODataConventionModelBuilder());

        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureAppSecurityApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddContentManagementApi(
        Action<ContentManagementConfiguration> configure = null)
    {
        ContentManagementConfiguration domain = new();
        ApplyCoreDefaults(domain);
        configure?.Invoke(domain);

        services.AddContentManagementWeb(
            configuration =>
            {
                ApplyConfiguration(domain, configuration);
                configuration.IncludeLegacyCoreContext = false;
            },
            new ODataConventionModelBuilder());

        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureContentManagementApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddDocumentManagementApi(
        Action<DocumentManagementConfiguration> configure = null)
    {
        DocumentManagementConfiguration domain = new();
        ApplyCoreDefaults(domain);
        configure?.Invoke(domain);

        services.AddDocumentManagementWeb(
            configuration =>
            {
                ApplyConfiguration(domain, configuration);
                configuration.IncludeLegacyCoreContext = false;
            },
            new ODataConventionModelBuilder());

        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureDocumentManagementApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddLoggingApi(
        Action<LoggingConfiguration> configure = null)
    {
        LoggingConfiguration domain = new();
        ApplyCoreDefaults(domain);
        configure?.Invoke(domain);

        services.AddLoggingWeb(
            configuration =>
            {
                ApplyConfiguration(domain, configuration);
                configuration.IncludeLegacyCoreContext = false;
            },
            new ODataConventionModelBuilder());

        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureLoggingApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddMailApi(
        Action<MailConfiguration> configure = null)
    {
        MailConfiguration domain = new();
        ApplyCoreDefaults(domain);
        configure?.Invoke(domain);

        services.AddMailWeb(
            configuration =>
            {
                ApplyConfiguration(domain, configuration);
                configuration.IncludeLegacyCoreContext = false;
            },
            new ODataConventionModelBuilder());

        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureMailApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddSchedulingApi(
        Action<SchedulingConfiguration> configure = null)
    {
        SchedulingConfiguration domain = new();
        ApplyCoreDefaults(domain);
        configure?.Invoke(domain);

        services.AddSchedulingWeb(
            configuration =>
            {
                ApplyConfiguration(domain, configuration);
                configuration.IncludeLegacyCoreContext = false;
            },
            new ODataConventionModelBuilder());

        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureSchedulingApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddWorkflowApi(
        Action<WorkflowConfiguration> configure = null)
    {
        WorkflowConfiguration domain = new();
        ApplyCoreDefaults(domain);
        configure?.Invoke(domain);

        services.AddWorkflowWeb(
            configuration =>
            {
                ApplyConfiguration(domain, configuration);
                configuration.IncludeLegacyCoreContext = false;
            },
            new ODataConventionModelBuilder());

        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureWorkflowApiModel());
        return this;
    }

    internal void Apply()
    {
        if (applied)
            return;

        ApplyCoreData();
        ApplySessionCacheFallback();
        ApplyHttpEventing();
        ApplyServiceBusEventing();
        services.AddCoreEventing(eventProviders);
        IEnumerable<CoreApiRouteDefinition> routes = EnsureRequiredRoutes(BuildRouteDefinitions());
        services.AddCoreApi(routes);
        services.AddCoreApiDocumentation(routes);
        RegisterApiInfos(routes);
        applied = true;
    }

    private static ContentManagementRuntimeConfig CreateContentManagementRuntimeConfig(Data.Config config) =>
        new()
        {
            ConnectionStrings = new Dictionary<string, string>(
                config.ConnectionStrings ?? new Dictionary<string, string>()),
            Settings = new Dictionary<string, string>(
                config.Settings ?? new Dictionary<string, string>()),
            Services = new Dictionary<string, string>(
                config.Services ?? new Dictionary<string, string>()),
            DebugInfo = config.DebugInfo,
            LogSQL = config.LogSQL,
        };

    private static MailRuntimeConfig CreateMailRuntimeConfig(Data.Config config) =>
        new()
        {
            ConnectionStrings = new Dictionary<string, string>(
                config.ConnectionStrings ?? new Dictionary<string, string>()),
            Settings = new Dictionary<string, string>(
                config.Settings ?? new Dictionary<string, string>()),
            Services = new Dictionary<string, string>(
                config.Services ?? new Dictionary<string, string>()),
            DebugInfo = config.DebugInfo,
            LogSQL = config.LogSQL,
        };

    private void ApplyCoreDefaults(AppSecurityConfiguration configuration) =>
        ApplyCoreDefaults(
            configuration.ConnectionStrings,
            configuration.Settings,
            configuration.Services,
            debugInfo: value => configuration.DebugInfo = value,
            logSql: value => configuration.LogSQL = value,
            configuration.DebugInfo,
            configuration.LogSQL);

    private void ApplyCoreDefaults(ContentManagementConfiguration configuration) =>
        ApplyCoreDefaults(
            configuration.ConnectionStrings,
            configuration.Settings,
            configuration.Services,
            debugInfo: value => configuration.DebugInfo = value,
            logSql: value => configuration.LogSQL = value,
            configuration.DebugInfo,
            configuration.LogSQL);

    private void ApplyCoreDefaults(DocumentManagementConfiguration configuration) =>
        ApplyCoreDefaults(
            configuration.ConnectionStrings,
            configuration.Settings,
            configuration.Services,
            debugInfo: value => configuration.DebugInfo = value,
            logSql: value => configuration.LogSQL = value,
            configuration.DebugInfo,
            configuration.LogSQL);

    private void ApplyCoreDefaults(LoggingConfiguration configuration) =>
        ApplyCoreDefaults(
            configuration.ConnectionStrings,
            configuration.Settings,
            configuration.Services,
            debugInfo: value => configuration.DebugInfo = value,
            logSql: value => configuration.LogSQL = value,
            configuration.DebugInfo,
            configuration.LogSQL);

    private void ApplyCoreDefaults(MailConfiguration configuration) =>
        ApplyCoreDefaults(
            configuration.ConnectionStrings,
            configuration.Settings,
            configuration.Services,
            debugInfo: value => configuration.DebugInfo = value,
            logSql: value => configuration.LogSQL = value,
            configuration.DebugInfo,
            configuration.LogSQL);

    private void ApplyCoreDefaults(SchedulingConfiguration configuration) =>
        ApplyCoreDefaults(
            configuration.ConnectionStrings,
            configuration.Settings,
            configuration.Services,
            debugInfo: value => configuration.DebugInfo = value,
            logSql: value => configuration.LogSQL = value,
            configuration.DebugInfo,
            configuration.LogSQL);

    private void ApplyCoreDefaults(WorkflowConfiguration configuration) =>
        ApplyCoreDefaults(
            configuration.ConnectionStrings,
            configuration.Settings,
            configuration.Services,
            debugInfo: value => configuration.DebugInfo = value,
            logSql: value => configuration.LogSQL = value,
            configuration.DebugInfo,
            configuration.LogSQL);

    private void ApplyCoreDefaults(
        IDictionary<string, string> connectionStrings,
        IDictionary<string, string> settings,
        IDictionary<string, string> servicesMap,
        Action<bool> debugInfo,
        Action<bool> logSql,
        bool currentDebugInfo,
        bool currentLogSql)
    {
        CoreConfigurationMapper.ApplyDefaults(
            coreConfiguration,
            connectionStrings,
            settings,
            servicesMap,
            debugInfo,
            logSql,
            currentDebugInfo,
            currentLogSql);
    }

    private void ApplySessionCacheFallback()
    {
        if (string.IsNullOrWhiteSpace(sessionCacheConnectionString))
            return;

        SqlSessionCacheFallback.UseInMemorySessionCacheUntilSqlSessionStoreExists(
            services,
            sessionCacheConnectionString);
    }

    private void ApplyCoreData()
    {
        services.AddCoreData(ResolveCoreConnectionString());
    }

    private void ApplyHttpEventing()
    {
        if (coreConfiguration?.EnableHttpEventing != true)
            return;

        services.AddHttpEventing(options =>
        {
            options.HubUrl = coreConfiguration.HttpEventHubUrl;
            options.MaxConcurrency = coreConfiguration.MaxConcurrency;
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });
    }

    private void ApplyServiceBusEventing()
    {
        if (coreConfiguration?.EnableServiceBusEventing != true)
            return;

        services.AddTransient<ServiceBusAppDeleteForwardingService>();
        services.AddTransient<ServiceBusFolderDeleteForwardingService>();

        services.AddAzureServiceBusEventing(options =>
        {
            options.ConnectionString = coreConfiguration.ServiceBusConnectionString;
            options.MaxConcurrency = coreConfiguration.MaxConcurrency;
        });
    }

    private void UseConfiguredExternalEventing(CoreConfiguration configuration)
    {
        if (configuration.EnableServiceBusEventing)
        {
            UseServiceBusEventing(configuration.ServiceBusConnectionString);
            return;
        }

        if (configuration.EnableHttpEventing || !string.IsNullOrWhiteSpace(configuration.HttpEventHubUrl))
        {
            UseHttpEventing(
                configuration.HttpEventHubUrl,
                options => options.MaxConcurrency = configuration.MaxConcurrency);
        }
    }

    private string ResolveCoreConnectionString()
    {
        string connectionString = coreConfiguration?.CoreConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A core database connection must be provided directly or available via core configuration.");
        }

        return connectionString;
    }

    private static Data.Config CreateRuntimeConfiguration(CoreConfiguration configuration) =>
        CoreConfigurationMapper.CreateRuntimeConfiguration(configuration);

    private CoreConfiguration EnsureCoreConfiguration() =>
        coreConfiguration ??= new CoreConfiguration();

    private static void ApplyConfiguration(
        AppSecurityConfiguration source,
        AppSecurityConfiguration target)
    {
        target.RootPath = source.RootPath;
        target.IncludeLegacyCoreContext = source.IncludeLegacyCoreContext;
        target.DebugInfo = source.DebugInfo;
        target.LogSQL = source.LogSQL;
        target.ConnectionStrings = new Dictionary<string, string>(source.ConnectionStrings, StringComparer.OrdinalIgnoreCase);
        target.Settings = new Dictionary<string, string>(source.Settings, StringComparer.OrdinalIgnoreCase);
        target.Services = new Dictionary<string, string>(source.Services, StringComparer.OrdinalIgnoreCase);
        CopyEventProviders(source.EventProviders, target.EventProviders);
    }

    private static void ApplyConfiguration(
        ContentManagementConfiguration source,
        ContentManagementConfiguration target)
    {
        target.RootPath = source.RootPath;
        target.IncludeLegacyCoreContext = source.IncludeLegacyCoreContext;
        target.DebugInfo = source.DebugInfo;
        target.LogSQL = source.LogSQL;
        target.ConnectionStrings = new Dictionary<string, string>(source.ConnectionStrings, StringComparer.OrdinalIgnoreCase);
        target.Settings = new Dictionary<string, string>(source.Settings, StringComparer.OrdinalIgnoreCase);
        target.Services = new Dictionary<string, string>(source.Services, StringComparer.OrdinalIgnoreCase);
        CopyEventProviders(source.EventProviders, target.EventProviders);
    }

    private static void ApplyConfiguration(
        DocumentManagementConfiguration source,
        DocumentManagementConfiguration target)
    {
        target.RootPath = source.RootPath;
        target.IncludeLegacyCoreContext = source.IncludeLegacyCoreContext;
        target.DebugInfo = source.DebugInfo;
        target.LogSQL = source.LogSQL;
        target.ConnectionStrings = new Dictionary<string, string>(source.ConnectionStrings, StringComparer.OrdinalIgnoreCase);
        target.Settings = new Dictionary<string, string>(source.Settings, StringComparer.OrdinalIgnoreCase);
        target.Services = new Dictionary<string, string>(source.Services, StringComparer.OrdinalIgnoreCase);
        CopyEventProviders(source.EventProviders, target.EventProviders);
    }

    private static void ApplyConfiguration(
        LoggingConfiguration source,
        LoggingConfiguration target)
    {
        target.RootPath = source.RootPath;
        target.IncludeLegacyCoreContext = source.IncludeLegacyCoreContext;
        target.DebugInfo = source.DebugInfo;
        target.LogSQL = source.LogSQL;
        target.ConnectionStrings = new Dictionary<string, string>(source.ConnectionStrings, StringComparer.OrdinalIgnoreCase);
        target.Settings = new Dictionary<string, string>(source.Settings, StringComparer.OrdinalIgnoreCase);
        target.Services = new Dictionary<string, string>(source.Services, StringComparer.OrdinalIgnoreCase);
        CopyEventProviders(source.EventProviders, target.EventProviders);
    }

    private static void ApplyConfiguration(
        MailConfiguration source,
        MailConfiguration target)
    {
        target.RootPath = source.RootPath;
        target.IncludeLegacyCoreContext = source.IncludeLegacyCoreContext;
        target.DebugInfo = source.DebugInfo;
        target.LogSQL = source.LogSQL;
        target.ConnectionStrings = new Dictionary<string, string>(source.ConnectionStrings, StringComparer.OrdinalIgnoreCase);
        target.Settings = new Dictionary<string, string>(source.Settings, StringComparer.OrdinalIgnoreCase);
        target.Services = new Dictionary<string, string>(source.Services, StringComparer.OrdinalIgnoreCase);
        CopyEventProviders(source.EventProviders, target.EventProviders);
    }

    private static void ApplyConfiguration(
        SchedulingConfiguration source,
        SchedulingConfiguration target)
    {
        target.RootPath = source.RootPath;
        target.IncludeLegacyCoreContext = source.IncludeLegacyCoreContext;
        target.DebugInfo = source.DebugInfo;
        target.LogSQL = source.LogSQL;
        target.ConnectionStrings = new Dictionary<string, string>(source.ConnectionStrings, StringComparer.OrdinalIgnoreCase);
        target.Settings = new Dictionary<string, string>(source.Settings, StringComparer.OrdinalIgnoreCase);
        target.Services = new Dictionary<string, string>(source.Services, StringComparer.OrdinalIgnoreCase);
        CopyEventProviders(source.EventProviders, target.EventProviders);
    }

    private static void ApplyConfiguration(
        WorkflowConfiguration source,
        WorkflowConfiguration target)
    {
        target.RootPath = source.RootPath;
        target.IncludeLegacyCoreContext = source.IncludeLegacyCoreContext;
        target.DebugInfo = source.DebugInfo;
        target.LogSQL = source.LogSQL;
        target.ConnectionStrings = new Dictionary<string, string>(source.ConnectionStrings, StringComparer.OrdinalIgnoreCase);
        target.Settings = new Dictionary<string, string>(source.Settings, StringComparer.OrdinalIgnoreCase);
        target.Services = new Dictionary<string, string>(source.Services, StringComparer.OrdinalIgnoreCase);
        CopyEventProviders(source.EventProviders, target.EventProviders);
    }

    private static void CopyEventProviders(
        IEnumerable<EventProvider> source,
        ICollection<EventProvider> target)
    {
        if (source is null || target is null)
            return;

        foreach (EventProvider provider in source)
            target.Add(provider);
    }
}
