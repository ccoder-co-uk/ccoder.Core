// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity;
using cCoder.ContentManagement;
using cCoder.Core.Exposures;
using cCoder.Core.Dependencies.Eventing;
using cCoder.Core.Brokers.ContentManagement;
using cCoder.Core.Brokers.Http;
using cCoder.Core.Exposures.Cors;
using cCoder.Core.Models;
using cCoder.Core.Services.Foundations.Eventing;
using cCoder.Core.Services.Foundations.AllowedOrigins;
using cCoder.Core.Brokers.Eventing;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Orchestrations;
using cCoder.Core.Services.Foundations.AppSecurity;
using cCoder.Core.Services.Processings.AllowedOrigins;
using cCoder.Data;
using cCoder.DocumentManagement.Models;
using cCoder.DocumentManagement;
using cCoder.Logging;
using cCoder.Logging.Models;
using cCoder.Mail;
using cCoder.Mail.Models;
using cCoder.Packaging;
using cCoder.Security;
using cCoder.Security.Data.EF;
using cCoder.AppSecurity.Models;
using cCoder.ContentManagement.Models;
using cCoder.Workflow;
using cCoder.Workflow.Models;
using cCoder.Eventing.Models;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.Http;
using cCoder.Eventing.Http.Models;
using Microsoft.OData.Edm;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json.Serialization;
using ContentManagementRuntimeConfig = cCoder.ContentManagement.Models.Config;
using MailRuntimeConfig = cCoder.Mail.Models.Config;


namespace cCoder.Core;

public partial class CoreBuilderOptions(IServiceCollection services)
{
    private readonly IServiceCollection services = services;
    private readonly List<EventProvider> eventProviders = [];
    private CoreConfiguration coreConfiguration;
    private string sessionCacheConnectionString;

    public CoreBuilderOptions WithCoreConfiguration(Action<CoreConfiguration> configure)
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

    public CoreBuilderOptions WithCoreConfiguration(Data.Config configuration)
    {
        configuration ??= new Data.Config();

        return WithCoreConfiguration(configure: coreConfig =>
            CoreConfigurationMapper.PopulateFromRuntimeConfiguration(target: coreConfig, source: configuration));
    }

    public CoreBuilderOptions WithEventProviders(params EventProvider[] eventProviders)
    {
        this.eventProviders.AddRange(collection: (eventProviders ?? []).Where(predicate: provider => provider is not null));
        return this;
    }

    public CoreBuilderOptions AddStorage(string connectionString = null)
    {
        if (!string.IsNullOrWhiteSpace(value: connectionString))
        {
            EnsureCoreConfiguration().CoreConnectionString = connectionString;
        }

        return this;
    }

    public CoreBuilderOptions WithSecurity(
        string connectionString,
        string decryptionKey)
    {
        cCoder.Security.IServiceCollectionExtensions.AddSecurity(services: services, configAction: (securityServices, securityConfig) =>
        {
            securityConfig.AddMSSQLModelProvider(
services: securityServices, connectionString: connectionString ?? string.Empty);

            securityConfig.UseAESHMMACPasswordEncryption(
services: securityServices, decryptionKey: decryptionKey ?? string.Empty);
        });

        return WithCoreConfiguration(configure: coreConfig =>
        {
            if (!string.IsNullOrWhiteSpace(value: connectionString))
            {
                coreConfig.SecurityConnectionString = connectionString;
            }

            if (!string.IsNullOrWhiteSpace(value: decryptionKey))
            {
                coreConfig.DecryptionKey = decryptionKey;
            }
        });
    }

    public CoreBuilderOptions UseHttpEventing() =>
        WithCoreConfiguration(configure: coreConfig => coreConfig.EnableHttpEventing = true);

    public CoreBuilderOptions UseServiceBusEventing() =>
        WithCoreConfiguration(configure: coreConfig => coreConfig.EnableServiceBusEventing = true);

    public CoreBuilderOptions WithSessionCache(string connectionString)
    {
        sessionCacheConnectionString = connectionString;
        return this;
    }

    public CoreBuilderOptions UseMSSQLProvider(string connectionString = null)
    {
        AddStorage(connectionString: connectionString);

        return this;
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

    public CoreBuilderOptions UseContentManagement(
        Action<ContentManagementConfiguration> configure = null,
        IDictionary<string, IEdmModel> map = null,
        bool servicesOnly = false
    )
    {
        services.AddContentManagementHostedServices(newContentManagementConfiguration: configuration =>
        {
            ApplyCoreDefaults(configuration: configuration);
            configure?.Invoke(obj: configuration);
        });

        services.TryAddTransient<IContentManagementAppBroker, ContentManagementAppBroker>();
        services.TryAddTransient<IHttpRequestBroker, HttpRequestBroker>();
        services.TryAddTransient<IContentManagementAppService, ContentManagementAppService>();
        services.TryAddTransient<IAllowedOriginStoreService, AllowedOriginStoreService>();
        services.TryAddTransient<IAllowedOriginStoreProcessingService, AllowedOriginStoreProcessingService>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IAppDomainProvider, Dependencies.Packaging.AppDomainProvider>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IAppSecurityPackageManagerBroker, Dependencies.Packaging.AppSecurityPackageManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IContentManagementPackageManagerBroker, Dependencies.Packaging.ContentManagementPackageManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IDocumentManagementPackageManagerBroker, Dependencies.Packaging.DocumentManagementPackageManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.ISchedulingPackageManagerBroker, Dependencies.Packaging.SchedulingPackageManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IWorkflowPackageManagerBroker, Dependencies.Packaging.WorkflowPackageManagerBroker>();
        services.AddPackaging();
        return this;
    }

    public CoreBuilderOptions UseDocumentManagement(
        Action<DocumentManagementConfiguration> configure = null)
    {
        services.AddDocumentManagementHostedServices(configure: configuration =>
        {
            ApplyCoreDefaults(configuration: configuration);
            configure?.Invoke(obj: configuration);
        });

        return this;
    }

    internal CoreBuilderOptions UseApi(IEnumerable<CoreApiRouteDefinition> routeDefinitions = null)
    {
        services.AddCoreApi(routeDefinitions: routeDefinitions);
        return this;
    }

    public CoreBuilderOptions UseApiDocumentation()
    {
        services.AddCoreApiDocumentation();
        return this;
    }

    public CoreBuilderOptions UseMail(Action<MailConfiguration> configure = null)
    {
        services.AddMailHostedServices(newMailConfiguration: configuration =>
        {
            ApplyCoreDefaults(configuration: configuration);
            configure?.Invoke(obj: configuration);
        });

        return this;
    }

    public CoreBuilderOptions UseWorkflow(Action<WorkflowConfiguration> configure = null)
    {
        services.AddWorkflowHostedServices(newConfigure: configuration =>
        {
            ApplyCoreDefaults(configuration: configuration);
            configure?.Invoke(obj: configuration);
        });

        return this;
    }

    public CoreBuilderOptions UseAppSecurity(Action<AppSecurityConfiguration> configure = null)
    {
        services.AddAppSecurityHostedServices(configure: configuration =>
        {
            ApplyCoreDefaults(configuration: configuration);
            configure?.Invoke(obj: configuration);
        });

        services.AddScoped<ICoreAllowedOriginStore, CoreAllowedOriginStore>();
        services.TryAddTransient<IAppSecurityAppService, AppSecurityAppService>();
        services.TryAddTransient<IAppSecurityUserRoleService, AppSecurityUserRoleService>();
        services.TryAddTransient<
            IHostedServicesAppSecurityAppAddOrchestrationService,
            HostedServicesAppSecurityAppAddOrchestrationService>();
        return this;
    }

    public CoreBuilderOptions UseLogging(Action<LoggingConfiguration> configure = null)
    {
        services.AddLoggingHostedServices(configure: configuration =>
        {
            ApplyCoreDefaults(configuration: configuration);
            configure?.Invoke(obj: configuration);
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
        configure?.Invoke(obj: configuration);
        CoreConfigurationMapper.ApplyBoundRootSections(target: configuration);

        WithCoreConfiguration(configure: coreConfig =>
            CoreConfigurationMapper.Copy(source: configuration, target: coreConfig));

        AddStorage(connectionString: configuration.CoreConnectionString);

        WithSecurity(
connectionString: configuration.SecurityConnectionString, decryptionKey: configuration.DecryptionKey);

        UseAppSecurity(configure: domain =>
        {
            ApplyConfiguration(source: configuration.AppSecurity, target: domain);
            domain.RootPath = configuration.AppSecurity.RootPath;
            domain.IncludeLegacyCoreContext = configuration.AppSecurity.IncludeLegacyCoreContext;
            domain.IsMigrating = configuration.AppSecurity.IsMigrating;
        });
        UseContentManagement(configure: domain =>
        {
            ApplyConfiguration(source: configuration.ContentManagement, target: domain);
            domain.RootPath = configuration.ContentManagement.RootPath;
            domain.IncludeLegacyCoreContext = configuration.ContentManagement.IncludeLegacyCoreContext;
        });
        UseDocumentManagement(configure: domain =>
        {
            ApplyConfiguration(source: configuration.DocumentManagement, target: domain);
            domain.RootPath = configuration.DocumentManagement.RootPath;
            domain.IncludeLegacyCoreContext = configuration.DocumentManagement.IncludeLegacyCoreContext;
        });
        UseLogging(configure: domain =>
        {
            ApplyConfiguration(source: configuration.DomainLogging, target: domain);
            domain.RootPath = configuration.DomainLogging.RootPath;
            domain.IncludeLegacyCoreContext = configuration.DomainLogging.IncludeLegacyCoreContext;
        });
        UseMail(configure: domain =>
        {
            ApplyConfiguration(source: configuration.Mail, target: domain);
            domain.RootPath = configuration.Mail.RootPath;
            domain.IncludeLegacyCoreContext = configuration.Mail.IncludeLegacyCoreContext;
            domain.IsMigrating = configuration.Mail.IsMigrating;
        });
        UseWorkflow(configure: domain =>
        {
            ApplyConfiguration(source: configuration.Workflow, target: domain);
            domain.RootPath = configuration.Workflow.RootPath;
            domain.IncludeLegacyCoreContext = configuration.Workflow.IncludeLegacyCoreContext;
            domain.IsMigrating = configuration.Workflow.IsMigrating;
        });
        UseConfiguredExternalEventing(configuration: configuration);
        WithEventProviders(eventProviders: configuration.EventProviders ?? []);

        return this;
    }

    public CoreBuilderOptions UseAll(Action<CoreConfiguration> configure) =>
        ConfigureDomainsWith(configure: configure);

    private static void ApplyConfiguration(
        AppSecurityConfiguration source,
        AppSecurityConfiguration target) =>
        ApplyConfiguration(
            source.ConnectionStrings,
            source.Settings,
            source.Services,
            source.DebugInfo,
            source.LogSQL,
            source.EventProviders,
            connectionStrings => MergeDictionary(target.ConnectionStrings, connectionStrings),
            settings => MergeDictionary(target.Settings, settings),
            services => MergeDictionary(target.Services, services),
            debugInfo => target.DebugInfo = debugInfo,
            logSql => target.LogSQL = logSql,
            eventProviders => target.EventProviders = eventProviders);

    private static void ApplyConfiguration(
        ContentManagementConfiguration source,
        ContentManagementConfiguration target) =>
        ApplyConfiguration(
            source.ConnectionStrings,
            source.Settings,
            source.Services,
            source.DebugInfo,
            source.LogSQL,
            source.EventProviders,
            connectionStrings => MergeDictionary(target.ConnectionStrings, connectionStrings),
            settings => MergeDictionary(target.Settings, settings),
            services => MergeDictionary(target.Services, services),
            debugInfo => target.DebugInfo = debugInfo,
            logSql => target.LogSQL = logSql,
            eventProviders => target.EventProviders = eventProviders);

    private static void ApplyConfiguration(
        DocumentManagementConfiguration source,
        DocumentManagementConfiguration target) =>
        ApplyConfiguration(
            source.ConnectionStrings,
            source.Settings,
            source.Services,
            source.DebugInfo,
            source.LogSQL,
            source.EventProviders,
            connectionStrings => MergeDictionary(target.ConnectionStrings, connectionStrings),
            settings => MergeDictionary(target.Settings, settings),
            services => MergeDictionary(target.Services, services),
            debugInfo => target.DebugInfo = debugInfo,
            logSql => target.LogSQL = logSql,
            eventProviders => target.EventProviders = eventProviders);

    private static void ApplyConfiguration(
        LoggingConfiguration source,
        LoggingConfiguration target) =>
        ApplyConfiguration(
            source.ConnectionStrings,
            source.Settings,
            source.Services,
            source.DebugInfo,
            source.LogSQL,
            source.EventProviders,
            connectionStrings => MergeDictionary(target.ConnectionStrings, connectionStrings),
            settings => MergeDictionary(target.Settings, settings),
            services => MergeDictionary(target.Services, services),
            debugInfo => target.DebugInfo = debugInfo,
            logSql => target.LogSQL = logSql,
            eventProviders => target.EventProviders = eventProviders);

    private static void ApplyConfiguration(
        MailConfiguration source,
        MailConfiguration target)
    {
        ApplyConfiguration(
            source.ConnectionStrings,
            source.Settings,
            source.Services,
            source.DebugInfo,
            source.LogSQL,
            source.EventProviders,
            connectionStrings => MergeDictionary(target.ConnectionStrings, connectionStrings),
            settings => MergeDictionary(target.Settings, settings),
            services => MergeDictionary(target.Services, services),
            debugInfo => target.DebugInfo = debugInfo,
            logSql => target.LogSQL = logSql,
            eventProviders => { });

        SetIfPresent(value: source.DefaultSenderProviderName, apply: value => target.DefaultSenderProviderName = value);
        SetIfPresent(value: source.DefaultReceiverProviderName, apply: value => target.DefaultReceiverProviderName = value);
        SetIfPresent(value: source.MicrosoftGraph.TenantId, apply: value => target.MicrosoftGraph.TenantId = value);
        SetIfPresent(value: source.MicrosoftGraph.ClientId, apply: value => target.MicrosoftGraph.ClientId = value);
        SetIfPresent(value: source.MicrosoftGraph.ClientSecret, apply: value => target.MicrosoftGraph.ClientSecret = value);
        SetIfPresent(value: source.MicrosoftGraph.GraphBaseUrl, apply: value => target.MicrosoftGraph.GraphBaseUrl = value);
        SetIfPresent(value: source.MicrosoftGraph.LoginBaseUrl, apply: value => target.MicrosoftGraph.LoginBaseUrl = value);
        SetIfPresent(value: source.MicrosoftGraph.ReceiveUser, apply: value => target.MicrosoftGraph.ReceiveUser = value);
    }

    private static void ApplyConfiguration(
        WorkflowConfiguration source,
        WorkflowConfiguration target) =>
        ApplyConfiguration(
            source.ConnectionStrings,
            source.Settings,
            source.Services,
            source.DebugInfo,
            source.LogSQL,
            source.EventProviders,
            connectionStrings => MergeDictionary(target.ConnectionStrings, connectionStrings),
            settings => MergeDictionary(target.Settings, settings),
            services => MergeDictionary(target.Services, services),
            debugInfo => target.DebugInfo = debugInfo,
            logSql => target.LogSQL = logSql,
            eventProviders => target.EventProviders = eventProviders);

    private static void ApplyConfiguration(
        IDictionary<string, string> connectionStrings,
        IDictionary<string, string> settings,
        IDictionary<string, string> services,
        bool debugInfo,
        bool logSql,
        EventProvider[] eventProviders,
        Action<IDictionary<string, string>> setConnectionStrings,
        Action<IDictionary<string, string>> setSettings,
        Action<IDictionary<string, string>> setServices,
        Action<bool> setDebugInfo,
        Action<bool> setLogSql,
        Action<EventProvider[]> setEventProviders)
    {
        setConnectionStrings(new Dictionary<string, string>(
            connectionStrings ?? new Dictionary<string, string>(),
            StringComparer.OrdinalIgnoreCase));
        setSettings(new Dictionary<string, string>(
            settings ?? new Dictionary<string, string>(),
            StringComparer.OrdinalIgnoreCase));
        setServices(new Dictionary<string, string>(
            services ?? new Dictionary<string, string>(),
            StringComparer.OrdinalIgnoreCase));
        setDebugInfo(debugInfo);
        setLogSql(logSql);
        setEventProviders(eventProviders is null ? [] : [.. eventProviders]);
    }

    private static void MergeDictionary(
        IDictionary<string, string> target,
        IDictionary<string, string> source)
    {
        if (target is null || source is null)
        {
            return;
        }

        foreach ((string key, string value) in source)
        {
            target[key] = value;
        }
    }

    private static void SetIfPresent(string value, Action<string> apply)
    {
        if (!string.IsNullOrWhiteSpace(value: value))
        {
            apply(obj: value);
        }
    }

    internal void Apply()
    {
        ApplyCoreData();
        ApplySessionCacheFallback();
        ApplyHttpEventing();
        ApplyServiceBusEventing();
        services.AddCoreEventing(eventProviders: eventProviders);
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

    private void ApplyCoreData() =>
        services.AddCoreData(connectionString: ResolveCoreConnectionString());

    private void ApplyHttpEventing()
    {
        if (coreConfiguration?.EnableHttpEventing != true)
        {
            return;
        }

        services.AddHttpEventingHostedServices(configure: options =>
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

        services.TryAddTransient<ServiceBusAppDeleteForwardingService>();
        services.TryAddTransient<ServiceBusFolderDeleteForwardingService>();

        services.TryAddTransient<
            IServiceBusEventingBroker,
            ServiceBusEventingDependency>();

        services.TryAddTransient<
            IServiceBusAppDeleteForwardingBroker,
            ServiceBusAppDeleteForwardingBroker>();

        services.TryAddTransient<
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
            UseServiceBusEventing();
            return;
        }

        if (configuration.EnableHttpEventing || !string.IsNullOrWhiteSpace(value: configuration.HttpEventHubUrl))
        {
            UseHttpEventing();
        }
    }

    private static Data.Config CreateRuntimeConfiguration(CoreConfiguration configuration) =>
        CoreConfigurationMapper.CreateRuntimeConfiguration(configuration: configuration);

    private CoreConfiguration EnsureCoreConfiguration() =>
        coreConfiguration ??= new CoreConfiguration();

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

}