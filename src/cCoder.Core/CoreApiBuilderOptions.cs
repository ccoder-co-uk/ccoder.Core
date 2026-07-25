// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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
using cCoder.Security;
using cCoder.Security.Data.EF;
using cCoder.Security.Exposures;
using cCoder.Security.Objects;
using cCoder.Core.Exposures;
using cCoder.Core.Services.Foundations.Eventing;
using cCoder.Core.Brokers.Eventing;
using cCoder.Workflow;
using cCoder.Workflow.Models;
using cCoder.Eventing.Models;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.Http;
using cCoder.Eventing.Http.Models;
using Microsoft.OData.ModelBuilder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        configure?.Invoke(obj: coreConfiguration);

        Data.Config runtimeConfiguration = CreateRuntimeConfiguration(configuration: coreConfiguration);
        services.AddSingleton(implementationInstance: coreConfiguration);
        services.AddSingleton(implementationInstance: runtimeConfiguration);
        services.AddSingleton(implementationInstance: CreateContentManagementRuntimeConfig(config: runtimeConfiguration));
        services.AddSingleton(implementationInstance: CreateMailRuntimeConfig(config: runtimeConfiguration));

        return this;
    }

    public CoreApiBuilderOptions WithCoreConfiguration(Data.Config configuration)
    {
        configuration ??= new Data.Config();

        return WithCoreConfiguration(configure: coreConfig =>
            CoreConfigurationMapper.PopulateFromRuntimeConfiguration(target: coreConfig, source: configuration));
    }

    public CoreApiBuilderOptions WithEventProviders(params EventProvider[] eventProviders)
    {
        this.eventProviders.AddRange(collection: (eventProviders ?? []).Where(predicate: provider => provider is not null));
        return this;
    }

    public CoreApiBuilderOptions AddStorage(string connectionString = null)
    {
        if (!string.IsNullOrWhiteSpace(value: connectionString))
        {
            EnsureCoreConfiguration().CoreConnectionString = connectionString;
        }

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
        WithCoreConfiguration(configure: coreConfig =>
        {
            if (!string.IsNullOrWhiteSpace(value: connectionString))
            {
                coreConfig.SecurityConnectionString = connectionString;
            }

            if (!string.IsNullOrWhiteSpace(value: decryptionKey))
            {
                coreConfig.DecryptionKey = decryptionKey;
            }

            if (!string.IsNullOrWhiteSpace(value: rootPath))
            {
                coreConfig.SecurityRootPath = rootPath;
            }
        });

    public CoreApiBuilderOptions UseHttpEventing(
        string hubUrl,
        Action<HttpEventingOptions> configure = null) =>
        WithCoreConfiguration(configure: coreConfig =>
        {
            coreConfig.EnableHttpEventing = true;

            if (!string.IsNullOrWhiteSpace(value: hubUrl))
            {
                coreConfig.HttpEventHubUrl = hubUrl;
            }

            if (configure is not null)
            {
                HttpEventingOptions eventingOptions = new();
                configure(obj: eventingOptions);
                coreConfig.MaxConcurrency = eventingOptions.MaxConcurrency;
            }
        });

    public CoreApiBuilderOptions UseServiceBusEventing(string connectionString) =>
        WithCoreConfiguration(configure: coreConfig =>
        {
            coreConfig.EnableServiceBusEventing = true;

            if (!string.IsNullOrWhiteSpace(value: connectionString))
            {
                coreConfig.ServiceBusConnectionString = connectionString;
            }
        });

    public CoreApiBuilderOptions AddSecurityApi(
        Action<IServiceCollection, SecurityConfiguration> configure = null)
    {
        string rootPath = "Api/Security";

        cCoder.Security.IServiceCollectionExtensions.AddSecurityApi(services: services, configAction: (securityServices, securityConfig) =>
        {
            securityConfig.RootPath = coreConfiguration?.SecurityRootPath ?? rootPath;

            securityConfig.AddMSSQLModelProvider(
services: securityServices, connectionString: coreConfiguration?.SecurityConnectionString ?? string.Empty);

            securityConfig.UseAESHMMACPasswordEncryption(
services: securityServices, decryptionKey: coreConfiguration?.DecryptionKey ?? string.Empty);

            configure?.Invoke(arg1: securityServices, arg2: securityConfig);
            rootPath = EnsureRoutePath(routePath: securityConfig.RootPath, defaultContext: "Security");
            securityConfig.RootPath = null;
        });

        RegisterContext(routePath: rootPath, configureModel: static builder => builder.ConfigureCoreSecurityApiModel());

        return this;
    }

    public CoreApiBuilderOptions AddAllDomains(string connection) =>
        AddAllDomains(configure: domains => domains.Connection = connection);

    public CoreApiBuilderOptions AddAllDomains(Action<CoreDomainsConfig> configure)
    {
        CoreDomainsConfig domains = new();
        configure(obj: domains);

        if (string.IsNullOrWhiteSpace(value: domains.Connection)
            && !string.IsNullOrWhiteSpace(value: coreConfiguration?.CoreConnectionString))
        {
            domains.Connection = coreConfiguration.CoreConnectionString;
        }

        if (string.IsNullOrWhiteSpace(value: domains.Connection))
        {
            throw new InvalidOperationException(
                "CoreDomainsConfig.Connection must be provided when adding the core business domains or available via core configuration.");
        }

        cCoder.Data.IServiceCollectionExtensions.AddCoreData(services: services, connectionString: domains.Connection);

        AddAppSecurityApi(configure: domain => ConfigureDomainRouting(configuration: domain, domainName: "AppSecurity", defaults: domains));
        AddContentManagementApi(configure: domain => ConfigureDomainRouting(configuration: domain, domainName: "ContentManagement", defaults: domains));
        AddDocumentManagementApi(configure: domain => ConfigureDomainRouting(configuration: domain, domainName: "DocumentManagement", defaults: domains));
        AddLoggingApi(configure: domain => ConfigureDomainRouting(configuration: domain, domainName: "Logging", defaults: domains));
        AddMailApi(configure: domain => ConfigureDomainRouting(configuration: domain, domainName: "Mail", defaults: domains));
        AddWorkflowApi(configure: domain => ConfigureDomainRouting(configuration: domain, domainName: "Workflow", defaults: domains));

        return this;
    }

    public CoreApiBuilderOptions UseLegacyCoreApi(string routePath = "Api/Core")
    {
        RegisterContext(routePath: routePath, configureModel: static builder => builder.ConfigureCoreAggregateApiModel());
        return this;
    }

    public CoreApiBuilderOptions UseLegacyCoreContext(string routePath = "Api/Core") =>
        UseLegacyCoreApi(routePath: routePath);

    public CoreApiBuilderOptions ConfigureDomainsWith(Action<CoreConfiguration> configure)
    {
        CoreConfiguration configuration = new();
        configure?.Invoke(obj: configuration);

        WithCoreConfiguration(configure: coreConfig =>
            CoreConfigurationMapper.Copy(source: configuration, target: coreConfig));

        AddStorage(connectionString: configuration.CoreConnectionString);

        WithSecurity(
connectionString: configuration.SecurityConnectionString, decryptionKey: configuration.DecryptionKey, rootPath: configuration.SecurityRootPath);

        AddAppSecurityApi();
        AddContentManagementApi();
        AddDocumentManagementApi();
        AddLoggingApi();
        AddMailApi();
        AddWorkflowApi();
        UseLegacyCoreApi();
        UseConfiguredExternalEventing(configuration: configuration);
        WithEventProviders(eventProviders: configuration.EventProviders ?? []);

        return this;
    }

    public CoreApiBuilderOptions UseAll(Action<CoreConfiguration> configure) =>
        ConfigureDomainsWith(configure: configure);

    public CoreApiBuilderOptions AddAppSecurityApi(
        Action<AppSecurityConfiguration> configure = null)
    {
        AppSecurityConfiguration domain = new();
        ApplyCoreDefaults(configuration: domain);
        configure?.Invoke(obj: domain);
        ApplyDomainRouteMode(configuration: domain, domainName: "AppSecurity");

        services.AddAppSecurityWeb(
configure: configuration =>
            {
                ApplyConfiguration(source: domain, target: configuration);
                ApplyDomainRouteMode(configuration: configuration, domainName: "AppSecurity");
                configuration.IncludeLegacyCoreContext = false;
            }, builder: new ODataConventionModelBuilder());

        RegisterDomainContext(
routePath: domain.RootPath, includeLegacyCoreContext: domain.IncludeLegacyCoreContext, configureModel: static builder => builder.ConfigureAppSecurityApiModel());

        return this;
    }

    public CoreApiBuilderOptions AddContentManagementApi(
        Action<ContentManagementConfiguration> configure = null)
    {
        ContentManagementConfiguration domain = new();
        ApplyCoreDefaults(configuration: domain);
        configure?.Invoke(obj: domain);
        ApplyDomainRouteMode(configuration: domain, domainName: "ContentManagement");

        services.AddContentManagementWeb(
newContentManagementConfiguration: configuration =>
            {
                ApplyConfiguration(source: domain, target: configuration);
                ApplyDomainRouteMode(configuration: configuration, domainName: "ContentManagement");
                configuration.IncludeLegacyCoreContext = false;
            }, builder: new ODataConventionModelBuilder());

        RegisterDomainContext(
routePath: domain.RootPath, includeLegacyCoreContext: domain.IncludeLegacyCoreContext, configureModel: static builder => builder.ConfigureContentManagementApiModel());

        return this;
    }

    public CoreApiBuilderOptions AddDocumentManagementApi(
        Action<DocumentManagementConfiguration> configure = null)
    {
        DocumentManagementConfiguration domain = new();
        ApplyCoreDefaults(configuration: domain);
        configure?.Invoke(obj: domain);
        ApplyDomainRouteMode(configuration: domain, domainName: "DocumentManagement");

        services.AddDocumentManagementWeb(
configure: configuration =>
            {
                ApplyConfiguration(source: domain, target: configuration);
                ApplyDomainRouteMode(configuration: configuration, domainName: "DocumentManagement");
                configuration.IncludeLegacyCoreContext = false;
            }, builder: new ODataConventionModelBuilder());

        RegisterDomainContext(
routePath: domain.RootPath, includeLegacyCoreContext: domain.IncludeLegacyCoreContext, configureModel: static builder => builder.ConfigureDocumentManagementApiModel());

        return this;
    }

    public CoreApiBuilderOptions AddLoggingApi(
        Action<LoggingConfiguration> configure = null)
    {
        LoggingConfiguration domain = new();
        ApplyCoreDefaults(configuration: domain);
        configure?.Invoke(obj: domain);
        ApplyDomainRouteMode(configuration: domain, domainName: "Logging");

        services.AddLoggingWeb(
configure: configuration =>
            {
                ApplyConfiguration(source: domain, target: configuration);
                ApplyDomainRouteMode(configuration: configuration, domainName: "Logging");
                configuration.IncludeLegacyCoreContext = false;
            }, builder: new ODataConventionModelBuilder());

        RegisterDomainContext(
routePath: domain.RootPath, includeLegacyCoreContext: domain.IncludeLegacyCoreContext, configureModel: static builder => builder.ConfigureLoggingApiModel());

        return this;
    }

    public CoreApiBuilderOptions AddMailApi(
        Action<MailConfiguration> configure = null)
    {
        MailConfiguration domain = new();
        ApplyCoreDefaults(configuration: domain);
        configure?.Invoke(obj: domain);
        ApplyDomainRouteMode(configuration: domain, domainName: "Mail");

        services.AddMailWeb(
newMailConfiguration: configuration =>
            {
                ApplyConfiguration(source: domain, target: configuration);
                ApplyDomainRouteMode(configuration: configuration, domainName: "Mail");
                configuration.IncludeLegacyCoreContext = false;
            }, builder: new ODataConventionModelBuilder());

        RegisterDomainContext(
routePath: domain.RootPath, includeLegacyCoreContext: domain.IncludeLegacyCoreContext, configureModel: static builder => builder.ConfigureMailApiModel());

        return this;
    }

    public CoreApiBuilderOptions AddWorkflowApi(
        Action<WorkflowConfiguration> configure = null)
    {
        WorkflowConfiguration domain = new();
        ApplyCoreDefaults(configuration: domain);
        configure?.Invoke(obj: domain);
        ApplyDomainRouteMode(configuration: domain, domainName: "Workflow");

        services.AddWorkflowWeb(
newConfigure: configuration =>
            {
                ApplyConfiguration(source: domain, target: configuration);
                ApplyDomainRouteMode(configuration: configuration, domainName: "Workflow");
                configuration.IncludeLegacyCoreContext = false;
            }, builder: new ODataConventionModelBuilder());

        RegisterDomainContext(
routePath: domain.RootPath, includeLegacyCoreContext: domain.IncludeLegacyCoreContext, configureModel: static builder => builder.ConfigureWorkflowApiModel());

        return this;
    }

    internal void Apply()
    {
        if (applied)
        {
            return;
        }

        ApplyCoreData();
        ApplySessionCacheFallback();
        ApplyHttpEventing();
        ApplyServiceBusEventing();
        services.AddCoreEventing(eventProviders: eventProviders);
        IEnumerable<CoreApiRouteDefinition> routes = EnsureRequiredRoutes(routes: BuildRouteDefinitions());
        services.AddCoreApi(routeDefinitions: routes);
        services.AddCoreApiDocumentation(routes: routes);
        RegisterApiInfos(routes: routes);
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
connectionStrings: configuration.ConnectionStrings, settings: configuration.Settings, servicesMap: configuration.Services, debugInfo: value => configuration.DebugInfo = value, logSql: value => configuration.LogSQL = value, currentDebugInfo: configuration.DebugInfo, currentLogSql: configuration.LogSQL);

    private void ApplyCoreDefaults(ContentManagementConfiguration configuration) =>
        ApplyCoreDefaults(
connectionStrings: configuration.ConnectionStrings, settings: configuration.Settings, servicesMap: configuration.Services, debugInfo: value => configuration.DebugInfo = value, logSql: value => configuration.LogSQL = value, currentDebugInfo: configuration.DebugInfo, currentLogSql: configuration.LogSQL);

    private void ApplyCoreDefaults(DocumentManagementConfiguration configuration) =>
        ApplyCoreDefaults(
connectionStrings: configuration.ConnectionStrings, settings: configuration.Settings, servicesMap: configuration.Services, debugInfo: value => configuration.DebugInfo = value, logSql: value => configuration.LogSQL = value, currentDebugInfo: configuration.DebugInfo, currentLogSql: configuration.LogSQL);

    private void ApplyCoreDefaults(LoggingConfiguration configuration) =>
        ApplyCoreDefaults(
connectionStrings: configuration.ConnectionStrings, settings: configuration.Settings, servicesMap: configuration.Services, debugInfo: value => configuration.DebugInfo = value, logSql: value => configuration.LogSQL = value, currentDebugInfo: configuration.DebugInfo, currentLogSql: configuration.LogSQL);

    private void ApplyCoreDefaults(MailConfiguration configuration)
    {
        ApplyCoreDefaults(
connectionStrings: configuration.ConnectionStrings, settings: configuration.Settings, servicesMap: configuration.Services, debugInfo: value => configuration.DebugInfo = value, logSql: value => configuration.LogSQL = value, currentDebugInfo: configuration.DebugInfo, currentLogSql: configuration.LogSQL);

        ApplyMailDefaults(configuration: configuration);
    }

    private void ApplyCoreDefaults(WorkflowConfiguration configuration) =>
        ApplyCoreDefaults(
connectionStrings: configuration.ConnectionStrings, settings: configuration.Settings, servicesMap: configuration.Services, debugInfo: value => configuration.DebugInfo = value, logSql: value => configuration.LogSQL = value, currentDebugInfo: configuration.DebugInfo, currentLogSql: configuration.LogSQL);

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
defaults: coreConfiguration, connectionStrings: connectionStrings, settings: settings, servicesMap: servicesMap, debugInfo: debugInfo, logSql: logSql, currentDebugInfo: currentDebugInfo, currentLogSql: currentLogSql);
    }

    private void ApplySessionCacheFallback()
    {
        if (string.IsNullOrWhiteSpace(value: sessionCacheConnectionString))
        {
            return;
        }

        if (SqlSessionTableExists(
            connectionString: sessionCacheConnectionString))
        {
            return;
        }

        services.AddOptions();

        services.Replace(
            descriptor: ServiceDescriptor.Singleton<
                IDistributedCache,
                MemoryDistributedCache>());
    }

    private static bool SqlSessionTableExists(string connectionString)
    {
        try
        {
            SqlConnectionStringBuilder builder = new(connectionString)
            {
                ConnectTimeout = 2,
            };

            using SqlConnection connection =
                new(builder.ConnectionString);

            connection.Open();

            using SqlCommand command = connection.CreateCommand();
            command.CommandTimeout = 2;
            command.CommandText = "SELECT OBJECT_ID(@tableName, 'U')";

            command.Parameters.AddWithValue(
                parameterName: "@tableName",
                value: "dbo.Sessions");

            object result = command.ExecuteScalar();

            return result is not null and not DBNull;
        }
        catch
        {
            return false;
        }
    }

    private void ApplyCoreData()
    {
        services.AddCoreData(connectionString: ResolveCoreConnectionString());
    }

    private void ApplyHttpEventing()
    {
        if (coreConfiguration?.EnableHttpEventing != true)
        {
            return;
        }

        services.AddHttpEventing(configure: options =>
        {
            options.HubUrl = coreConfiguration.HttpEventHubUrl;
            options.MaxConcurrency = coreConfiguration.MaxConcurrency;
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });
    }

    private void ApplyServiceBusEventing()
    {
        if (coreConfiguration?.EnableServiceBusEventing != true)
        {
            return;
        }

        services.AddTransient<ServiceBusAppDeleteForwardingService>();
        services.AddTransient<ServiceBusFolderDeleteForwardingService>();

        services.AddTransient<
            IServiceBusAppDeleteForwardingBroker,
            ServiceBusAppDeleteForwardingBroker>();

        services.AddTransient<
            IServiceBusFolderDeleteForwardingBroker,
            ServiceBusFolderDeleteForwardingBroker>();

        services.AddAzureServiceBusEventing(configure: options =>
        {
            options.ConnectionString = coreConfiguration.ServiceBusConnectionString;
            options.MaxConcurrency = coreConfiguration.MaxConcurrency;
        });
    }

    private void UseConfiguredExternalEventing(CoreConfiguration configuration)
    {
        if (configuration.EnableServiceBusEventing)
        {
            UseServiceBusEventing(connectionString: configuration.ServiceBusConnectionString);
            return;
        }

        if (configuration.EnableHttpEventing || !string.IsNullOrWhiteSpace(value: configuration.HttpEventHubUrl))
        {
            UseHttpEventing(
hubUrl: configuration.HttpEventHubUrl, configure: options => options.MaxConcurrency = configuration.MaxConcurrency);
        }
    }

    private string ResolveCoreConnectionString()
    {
        string connectionString = coreConfiguration?.CoreConnectionString;

        if (string.IsNullOrWhiteSpace(value: connectionString))
        {
            throw new InvalidOperationException(
                "A core database connection must be provided directly or available via core configuration.");
        }

        return connectionString;
    }

    private static Data.Config CreateRuntimeConfiguration(CoreConfiguration configuration) =>
        CoreConfigurationMapper.CreateRuntimeConfiguration(configuration: configuration);

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
        target.ConnectionStrings = CopyDictionary(source: source.ConnectionStrings);
        target.Settings = CopyDictionary(source: source.Settings);
        target.Services = CopyDictionary(source: source.Services);
        CopyEventProviders(source: source.EventProviders, target: target.EventProviders);
    }

    private static void ApplyConfiguration(
        ContentManagementConfiguration source,
        ContentManagementConfiguration target)
    {
        target.RootPath = source.RootPath;
        target.IncludeLegacyCoreContext = source.IncludeLegacyCoreContext;
        target.DebugInfo = source.DebugInfo;
        target.LogSQL = source.LogSQL;
        target.ConnectionStrings = CopyDictionary(source: source.ConnectionStrings);
        target.Settings = CopyDictionary(source: source.Settings);
        target.Services = CopyDictionary(source: source.Services);
        CopyEventProviders(source: source.EventProviders, target: target.EventProviders);
    }

    private static void ApplyConfiguration(
        DocumentManagementConfiguration source,
        DocumentManagementConfiguration target)
    {
        target.RootPath = source.RootPath;
        target.IncludeLegacyCoreContext = source.IncludeLegacyCoreContext;
        target.DebugInfo = source.DebugInfo;
        target.LogSQL = source.LogSQL;
        target.ConnectionStrings = CopyDictionary(source: source.ConnectionStrings);
        target.Settings = CopyDictionary(source: source.Settings);
        target.Services = CopyDictionary(source: source.Services);
        CopyEventProviders(source: source.EventProviders, target: target.EventProviders);
    }

    private static void ApplyConfiguration(
        LoggingConfiguration source,
        LoggingConfiguration target)
    {
        target.RootPath = source.RootPath;
        target.IncludeLegacyCoreContext = source.IncludeLegacyCoreContext;
        target.DebugInfo = source.DebugInfo;
        target.LogSQL = source.LogSQL;
        target.ConnectionStrings = CopyDictionary(source: source.ConnectionStrings);
        target.Settings = CopyDictionary(source: source.Settings);
        target.Services = CopyDictionary(source: source.Services);
        CopyEventProviders(source: source.EventProviders, target: target.EventProviders);
    }

    private static void ApplyConfiguration(
        MailConfiguration source,
        MailConfiguration target)
    {
        target.RootPath = source.RootPath;
        target.IncludeLegacyCoreContext = source.IncludeLegacyCoreContext;
        target.DebugInfo = source.DebugInfo;
        target.LogSQL = source.LogSQL;
        target.ConnectionStrings = CopyDictionary(source: source.ConnectionStrings);
        target.Settings = CopyDictionary(source: source.Settings);
        target.Services = CopyDictionary(source: source.Services);
        target.MicrosoftGraph.TenantId = source.MicrosoftGraph.TenantId;
        target.MicrosoftGraph.ClientId = source.MicrosoftGraph.ClientId;
        target.MicrosoftGraph.ClientSecret = source.MicrosoftGraph.ClientSecret;
        target.MicrosoftGraph.GraphBaseUrl = source.MicrosoftGraph.GraphBaseUrl;
        target.MicrosoftGraph.LoginBaseUrl = source.MicrosoftGraph.LoginBaseUrl;
        target.MicrosoftGraph.ReceiveUser = source.MicrosoftGraph.ReceiveUser;
        target.DefaultSenderProviderName = source.DefaultSenderProviderName;
        target.DefaultReceiverProviderName = source.DefaultReceiverProviderName;
        CopyEventProviders(source: source.EventProviders, target: target.EventProviders);
    }

    private static void ApplyConfiguration(
        WorkflowConfiguration source,
        WorkflowConfiguration target)
    {
        target.RootPath = source.RootPath;
        target.IncludeLegacyCoreContext = source.IncludeLegacyCoreContext;
        target.DebugInfo = source.DebugInfo;
        target.LogSQL = source.LogSQL;
        target.ConnectionStrings = CopyDictionary(source: source.ConnectionStrings);
        target.Settings = CopyDictionary(source: source.Settings);
        target.Services = CopyDictionary(source: source.Services);
        CopyEventProviders(source: source.EventProviders, target: target.EventProviders);
    }

    private static void CopyEventProviders(
        IEnumerable<EventProvider> source,
        ICollection<EventProvider> target)
    {
        if (source is null || target is null)
        {
            return;
        }

        foreach (EventProvider provider in source)
        {
            target.Add(item: provider);
        }
    }

    private static Dictionary<string, string> CopyDictionary(
        IDictionary<string, string> source) =>
        new(
            dictionary: source ?? new Dictionary<string, string>(),
            comparer: StringComparer.OrdinalIgnoreCase);

    private void ApplyMailDefaults(MailConfiguration configuration)
    {
        if (coreConfiguration is null)
        {
            return;
        }

        SetIfPresent(value: coreConfiguration.MailGraphTenantId, apply: value => configuration.MicrosoftGraph.TenantId = value);
        SetIfPresent(value: coreConfiguration.MailGraphClientId, apply: value => configuration.MicrosoftGraph.ClientId = value);
        SetIfPresent(value: coreConfiguration.MailGraphClientSecret, apply: value => configuration.MicrosoftGraph.ClientSecret = value);
        SetIfPresent(value: coreConfiguration.MailGraphBaseUrl, apply: value => configuration.MicrosoftGraph.GraphBaseUrl = value);
        SetIfPresent(value: coreConfiguration.MailGraphLoginBaseUrl, apply: value => configuration.MicrosoftGraph.LoginBaseUrl = value);
        SetIfPresent(value: coreConfiguration.MailGraphReceiveUser, apply: value => configuration.MicrosoftGraph.ReceiveUser = value);
        SetIfPresent(value: coreConfiguration.MailDefaultSenderProviderName, apply: value => configuration.DefaultSenderProviderName = value);
        SetIfPresent(value: coreConfiguration.MailDefaultReceiverProviderName, apply: value => configuration.DefaultReceiverProviderName = value);
    }

    private static void SetIfPresent(string value, Action<string> apply)
    {
        if (!string.IsNullOrWhiteSpace(value: value))
        {
            apply(obj: value);
        }
    }
}