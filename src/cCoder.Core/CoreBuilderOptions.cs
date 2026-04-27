using cCoder.AppSecurity;
using cCoder.AppSecurity.Exposures.HostedServices;
using cCoder.ContentManagement;
using cCoder.Core.Api;
using cCoder.Core.Cors;
using cCoder.Core.Models;
using cCoder.Data;
using cCoder.DocumentManagement.Models;
using cCoder.DocumentManagement;
using cCoder.Logging.Models;
using cCoder.Mail;
using cCoder.Mail.Exposures.HostedServices;
using cCoder.Mail.Models;
using cCoder.Packaging;
using cCoder.Scheduling;
using cCoder.Scheduling.Exposures.HostedServices;
using cCoder.Scheduling.Models;
using cCoder.Security;
using cCoder.Security.Api;
using cCoder.Security.Data.EF.MSSQL;
using cCoder.AppSecurity.Models;
using cCoder.ContentManagement.Models;
using cCoder.Workflow;
using cCoder.Workflow.Exposures.HostedServices;
using cCoder.Workflow.Models;
using cCoder.Eventing.Models;
using Microsoft.OData.Edm;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DataConfig = cCoder.Data.Config;
using ContentManagementRuntimeConfig = cCoder.ContentManagement.Models.Config;
using MailRuntimeConfig = cCoder.Mail.Models.Config;


namespace cCoder.Core;

public partial class CoreBuilderOptions
{
    private readonly IServiceCollection services;
    private readonly List<EventProvider> eventProviders = [];
    private DataConfig coreConfiguration;
    private string sessionCacheConnectionString;

    public CoreBuilderOptions(IServiceCollection services) => 
        this.services = services;

    public CoreBuilderOptions WithCoreConfiguration(DataConfig configuration)
    {
        coreConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        services.AddSingleton(configuration);
        services.AddSingleton(CreateContentManagementRuntimeConfig(configuration));
        services.AddSingleton(CreateMailRuntimeConfig(configuration));

        return this;
    }

    public CoreBuilderOptions WithEventProviders(params EventProvider[] eventProviders)
    {
        this.eventProviders.AddRange((eventProviders ?? []).Where(provider => provider is not null));
        return this;
    }

    public CoreBuilderOptions WithSecurity(
        string connectionString,
        string decryptionKey)
    {
        services.AddSecurity((securityServices, securityConfig) =>
        {
            securityConfig.AddMSSQLModelProvider(
                securityServices,
                connectionString);

            securityConfig.UseAESHMMACPasswordEncryption(
                securityServices,
                decryptionKey);
        });

        return this;
    }

    public CoreBuilderOptions WithSessionCache(string connectionString)
    {
        sessionCacheConnectionString = connectionString;
        return this;
    }

    public CoreBuilderOptions UseMSSQLProvider(string connectionString = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString)
            && coreConfiguration?.ConnectionStrings is not null
            && coreConfiguration.ConnectionStrings.TryGetValue("Core", out string coreConnection))
        {
            connectionString = coreConnection;
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A core database connection must be provided directly or available via core configuration.");
        }

        services.AddCoreData(connectionString);
        return this;
    }

    public CoreBuilderOptions UseContentManagement(
        Action<ContentManagementConfiguration> configure = null,
        IDictionary<string, IEdmModel> map = null,
        bool servicesOnly = false
    )
    {
        services.AddContentManagement((_, configuration) =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
        services.TryAddTransient<cCoder.Packaging.Brokers.IAppDomainProvider, Brokers.Packaging.AppDomainProvider>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IAppSecurityPackageManagerBroker, Brokers.Packaging.AppSecurityPackageManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IContentManagementPackageManagerBroker, Brokers.Packaging.ContentManagementPackageManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IDocumentManagementPackageManagerBroker, Brokers.Packaging.DocumentManagementPackageManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.ISchedulingPackageManagerBroker, Brokers.Packaging.SchedulingPackageManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IWorkflowPackageManagerBroker, Brokers.Packaging.WorkflowPackageManagerBroker>();
        services.AddPackaging();
        return this;
    }

    public CoreBuilderOptions UseDocumentManagement(
        Action<DocumentManagementConfiguration> configure = null)
    {
        services.AddDocumentManagement((_, configuration) =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
        return this;
    }

    internal CoreBuilderOptions UseApi(IEnumerable<CoreApiRouteDefinition> routeDefinitions = null)
    {
        services.AddCoreApi(routeDefinitions);
        return this;
    }

    public CoreBuilderOptions UseApiDocumentation()
    {
        services.AddCoreApiDocumentation();
        return this;
    }

    public CoreBuilderOptions UseMail(Action<MailConfiguration> configure = null)
    {
        services.AddMail((_, configuration) =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
        services.AddHostedService<MailSenderHostedService>();
        return this;
    }

    public CoreBuilderOptions UseScheduling(Action<SchedulingConfiguration> configure = null)
    {
        services.AddScheduling((_, configuration) =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
        services.AddHostedService<TaskRunnerHostedService>();
        return this;
    }

    public CoreBuilderOptions UseWorkflow(Action<WorkflowConfiguration> configure = null)
    {
        services.AddWorkflowHostedServices((_, configuration) =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
        return this;
    }

    public CoreBuilderOptions UseAppSecurity(Action<AppSecurityConfiguration> configure = null)
    {
        services.AddAppSecurityHostedServices((_, configuration) =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
        services.AddSingleton<ICoreAllowedOriginStore, CoreAllowedOriginStore>();
        return this;
    }

    public CoreBuilderOptions UseLogging(Action<LoggingConfiguration> configure = null)
    {
        cCoder.Logging.LoggingServiceCollectionConfigurationExtensions.AddLogging(services, (_, configuration) =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
        return this;
    }

    public CoreBuilderOptions AuthorizeUsersWith()
    {
        services.AddCoreAuthInfo();
        return this;
    }

    internal void Apply()
    {
        ApplySessionCacheFallback();
        services.AddCoreHostedEventing(eventProviders);
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
}
