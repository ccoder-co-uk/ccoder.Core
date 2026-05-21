using cCoder.AppSecurity;
using cCoder.ContentManagement;
using cCoder.Core.Api;
using cCoder.Core.Cors;
using cCoder.Core.Models;
using cCoder.Data;
using cCoder.DocumentManagement.Models;
using cCoder.DocumentManagement;
using cCoder.Logging;
using cCoder.Logging.Models;
using cCoder.Mail;
using cCoder.Mail.Models;
using cCoder.Packaging;
using cCoder.Scheduling;
using cCoder.Scheduling.Models;
using cCoder.Security;
using cCoder.Security.Api;
using cCoder.Security.Data.EF.MSSQL;
using cCoder.AppSecurity.Models;
using cCoder.ContentManagement.Models;
using cCoder.Workflow;
using cCoder.Workflow.Models;
using cCoder.Eventing.Models;
using cCoder.Eventing.Http;
using cCoder.Eventing.Http.Models;
using Microsoft.OData.Edm;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json.Serialization;
using ContentManagementRuntimeConfig = cCoder.ContentManagement.Models.Config;
using MailRuntimeConfig = cCoder.Mail.Models.Config;


namespace cCoder.Core;

public partial class CoreBuilderOptions
{
    private readonly IServiceCollection services;
    private readonly List<EventProvider> eventProviders = [];
    private CoreConfiguration coreConfiguration;
    private string sessionCacheConnectionString;

    public CoreBuilderOptions(IServiceCollection services) => 
        this.services = services;

    public CoreBuilderOptions WithCoreConfiguration(Action<CoreConfiguration> configure)
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

    public CoreBuilderOptions WithCoreConfiguration(Data.Config configuration)
    {
        configuration ??= new Data.Config();

        return WithCoreConfiguration(coreConfig =>
            CoreConfigurationMapper.PopulateFromRuntimeConfiguration(coreConfig, configuration));
    }

    public CoreBuilderOptions WithEventProviders(params EventProvider[] eventProviders)
    {
        this.eventProviders.AddRange((eventProviders ?? []).Where(provider => provider is not null));
        return this;
    }

    public CoreBuilderOptions AddStorage(string connectionString = null)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
            EnsureCoreConfiguration().CoreConnectionString = connectionString;

        return this;
    }

    public CoreBuilderOptions WithSecurity(
        string connectionString,
        string decryptionKey)
    {
        cCoder.Security.IServiceCollectionExtensions.AddSecurity(services, (securityServices, securityConfig) =>
        {
            securityConfig.AddMSSQLModelProvider(
                securityServices,
                connectionString ?? string.Empty);
            securityConfig.UseAESHMMACPasswordEncryption(
                securityServices,
                decryptionKey ?? string.Empty);
        });

        return WithCoreConfiguration(coreConfig =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
                coreConfig.SecurityConnectionString = connectionString;

            if (!string.IsNullOrWhiteSpace(decryptionKey))
                coreConfig.DecryptionKey = decryptionKey;
        });
    }

    public CoreBuilderOptions UseHttpEventing() =>
        WithCoreConfiguration(coreConfig => coreConfig.EnableHttpEventing = true);

    public CoreBuilderOptions WithSessionCache(string connectionString)
    {
        sessionCacheConnectionString = connectionString;
        return this;
    }

    public CoreBuilderOptions UseMSSQLProvider(string connectionString = null)
    {
        AddStorage(connectionString);

        return this;
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

    public CoreBuilderOptions UseContentManagement(
        Action<ContentManagementConfiguration> configure = null,
        IDictionary<string, IEdmModel> map = null,
        bool servicesOnly = false
    )
    {
        services.AddContentManagementHostedServices(configuration =>
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
        services.AddDocumentManagementHostedServices(configuration =>
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
        services.AddMailHostedServices(configuration =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
        return this;
    }

    public CoreBuilderOptions UseScheduling(Action<SchedulingConfiguration> configure = null)
    {
        services.AddSchedulingHostedServices(configuration =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
        return this;
    }

    public CoreBuilderOptions UseWorkflow(Action<WorkflowConfiguration> configure = null)
    {
        services.AddWorkflowHostedServices(configuration =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
        return this;
    }

    public CoreBuilderOptions UseAppSecurity(Action<AppSecurityConfiguration> configure = null)
    {
        services.AddAppSecurityHostedServices(configuration =>
        {
            ApplyCoreDefaults(configuration);
            configure?.Invoke(configuration);
        });
        services.AddSingleton<ICoreAllowedOriginStore, CoreAllowedOriginStore>();
        services.TryAddTransient<HostedServicesAppSecurityAppAddOrchestrationService>();
        return this;
    }

    public CoreBuilderOptions UseLogging(Action<LoggingConfiguration> configure = null)
    {
        services.AddLoggingHostedServices(configuration =>
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

    public CoreBuilderOptions ConfigureDomainsWith(Action<CoreConfiguration> configure)
    {
        CoreConfiguration configuration = new();
        configure?.Invoke(configuration);

        WithCoreConfiguration(coreConfig =>
            CoreConfigurationMapper.Copy(configuration, coreConfig));

        AddStorage(configuration.CoreConnectionString);
        WithSecurity(
            configuration.SecurityConnectionString,
            configuration.DecryptionKey);
        UseAppSecurity();
        UseContentManagement();
        UseDocumentManagement();
        UseLogging();
        UseMail();
        UseScheduling();
        UseWorkflow();
        UseHttpEventing();
        WithEventProviders(configuration.EventProviders ?? []);

        return this;
    }

    public CoreBuilderOptions UseAll(Action<CoreConfiguration> configure) =>
        ConfigureDomainsWith(configure);

    internal void Apply()
    {
        ApplyCoreData();
        ApplySessionCacheFallback();
        ApplyHttpEventing();
        services.AddCoreEventing(eventProviders);
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

    private void ApplyCoreData() =>
        services.AddCoreData(ResolveCoreConnectionString());

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

    private static Data.Config CreateRuntimeConfiguration(CoreConfiguration configuration) =>
        CoreConfigurationMapper.CreateRuntimeConfiguration(configuration);

    private CoreConfiguration EnsureCoreConfiguration() =>
        coreConfiguration ??= new CoreConfiguration();
}
