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
using cCoder.Security.Objects;
using cCoder.Core.Api;
using cCoder.Workflow;
using cCoder.Workflow.Models;
using cCoder.Eventing.Models;
using Microsoft.OData.ModelBuilder;
using DataConfig = cCoder.Data.Config;
using ContentManagementRuntimeConfig = cCoder.ContentManagement.Models.Config;
using MailRuntimeConfig = cCoder.Mail.Models.Config;

namespace cCoder.Core;

public partial class CoreApiBuilderOptions
{
    private readonly Dictionary<string, List<Action<ODataConventionModelBuilder>>> routeContributors =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly List<EventProvider> eventProviders = [];

    private readonly IServiceCollection services;
    private DataConfig coreConfiguration;
    private string sessionCacheConnectionString;
    private bool applied;

    public CoreApiBuilderOptions(IServiceCollection services) =>
        this.services = services;

    public CoreApiBuilderOptions WithCoreConfiguration(DataConfig configuration)
    {
        coreConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        services.AddSingleton(configuration);
        services.AddSingleton(CreateContentManagementRuntimeConfig(configuration));
        services.AddSingleton(CreateMailRuntimeConfig(configuration));

        return this;
    }

    public CoreApiBuilderOptions WithEventProviders(params EventProvider[] eventProviders)
    {
        this.eventProviders.AddRange((eventProviders ?? []).Where(provider => provider is not null));
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
        string rootPath = "Api/Security")
    {
        AddSecurityApi((securityServices, securityConfig) =>
        {
            securityConfig.RootPath = rootPath;
            securityConfig.AddMSSQLModelProvider(
                securityServices,
                connectionString);
            securityConfig.UseAESHMMACPasswordEncryption(
                securityServices,
                decryptionKey);
        });

        return this;
    }

    public CoreApiBuilderOptions AddSecurityApi(
        Action<IServiceCollection, SecurityConfiguration> configure = null)
    {
        string rootPath = "Api/Security";

        services.AddSecurityApi((securityServices, securityConfig) =>
        {
            securityConfig.RootPath = rootPath;
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
            && coreConfiguration?.ConnectionStrings is not null
            && coreConfiguration.ConnectionStrings.TryGetValue("Core", out string coreConnection))
        {
            domains.Connection = coreConnection;
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

    public CoreApiBuilderOptions UseLegacyCoreContext(string routePath = "Api/Core")
    {
        RegisterContext(routePath, static builder => builder.ConfigureCoreAggregateApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddAppSecurityApi(
        Action<AppSecurityConfiguration> configure = null)
    {
        AppSecurityConfiguration domain = services.AddAppSecurity((_, configuration) =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureAppSecurityApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddContentManagementApi(
        Action<ContentManagementConfiguration> configure = null)
    {
        ContentManagementConfiguration domain = services.AddContentManagement((_, configuration) =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureContentManagementApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddDocumentManagementApi(
        Action<DocumentManagementConfiguration> configure = null)
    {
        DocumentManagementConfiguration domain = services.AddDocumentManagement((_, configuration) =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureDocumentManagementApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddLoggingApi(
        Action<LoggingConfiguration> configure = null)
    {
        LoggingConfiguration domain = services.AddLogging((_, configuration) =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureLoggingApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddMailApi(
        Action<MailConfiguration> configure = null)
    {
        MailConfiguration domain = services.AddMail((_, configuration) =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureMailApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddSchedulingApi(
        Action<SchedulingConfiguration> configure = null)
    {
        SchedulingConfiguration domain = services.AddScheduling((_, configuration) =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureSchedulingApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddWorkflowApi(
        Action<WorkflowConfiguration> configure = null)
    {
        WorkflowConfiguration domain = services.AddWorkflow((_, configuration) =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
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
        services.AddCoreApiEventing(eventProviders);
        IEnumerable<CoreApiRouteDefinition> routes = BuildRouteDefinitions();
        cCoder.Core.Api.IServiceCollectionExtensions.AddCoreApi(services, routes);
        services.AddCoreApiDocumentation(routes);
        RegisterApiInfos(routes);
        applied = true;
    }

    private static ContentManagementRuntimeConfig CreateContentManagementRuntimeConfig(DataConfig config) =>
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

    private static MailRuntimeConfig CreateMailRuntimeConfig(DataConfig config) =>
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
        if (coreConfiguration is null)
            return;

        MergeMissingEntries(connectionStrings, coreConfiguration.ConnectionStrings);
        MergeMissingEntries(settings, coreConfiguration.Settings);
        MergeMissingEntries(servicesMap, coreConfiguration.Services);
        debugInfo(currentDebugInfo || coreConfiguration.DebugInfo);
        logSql(currentLogSql || coreConfiguration.LogSQL);
    }

    private static void MergeMissingEntries(
        IDictionary<string, string> target,
        IDictionary<string, string> defaults)
    {
        if (target is null || defaults is null)
            return;

        foreach ((string key, string value) in defaults)
        {
            if (!target.ContainsKey(key))
                target[key] = value;
        }
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
        if (coreConfiguration?.ConnectionStrings is null
            || !coreConfiguration.ConnectionStrings.TryGetValue("Core", out string connectionString)
            || string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        services.AddCoreData(connectionString);
    }
}
