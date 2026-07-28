// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity;
using cCoder.AppSecurity.Models;
using cCoder.ContentManagement;
using cCoder.ContentManagement.Models;
using cCoder.Core.Models;
using cCoder.Core.Dependencies.Eventing;
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

namespace cCoder.Core;

public partial class CoreApiBuilderOptions(IServiceCollection services)
{
    private readonly Dictionary<string, List<Action<ODataConventionModelBuilder>>> routeContributors =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly List<EventProvider> eventProviders = [];

    private readonly IServiceCollection services = services;
    private CoreConfiguration coreConfiguration;
    private string sessionCacheConnectionString;
    private bool applied;

    public CoreApiBuilderOptions WithCoreConfiguration(Action<CoreConfiguration> configure)
    {
        coreConfiguration ??= new CoreConfiguration();
        configure?.Invoke(obj: coreConfiguration);
        services.AddSingleton(implementationInstance: coreConfiguration);

        return this;
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
            CoreConfiguration configuration = EnsureCoreConfiguration();
            configuration.AppSecurity.ConnectionString = connectionString;
            configuration.ContentManagement.ConnectionString = connectionString;
            configuration.DocumentManagement.ConnectionString = connectionString;
            configuration.Logging.ConnectionString = connectionString;
            configuration.Mail.ConnectionString = connectionString;
            configuration.Workflow.ConnectionString = connectionString;
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
                coreConfig.Security.ConnectionString = connectionString;
            }

            if (!string.IsNullOrWhiteSpace(value: decryptionKey))
            {
                coreConfig.Security.DecryptionKey = decryptionKey;
            }

            if (!string.IsNullOrWhiteSpace(value: rootPath))
            {
                coreConfig.Security.RootPath = rootPath;
            }
        });

    public CoreApiBuilderOptions UseHttpEventing(
        string hubUrl,
        Action<HttpEventingOptions> configure = null) =>
        WithCoreConfiguration(configure: coreConfig =>
        {
            coreConfig.Eventing.ProviderType = "Http";

            if (!string.IsNullOrWhiteSpace(value: hubUrl))
            {
                coreConfig.Eventing.Http.HubUrl = hubUrl;
            }

            if (configure is not null)
            {
                HttpEventingOptions eventingOptions = new();
                configure(obj: eventingOptions);
                coreConfig.Eventing.Http.MaxConcurrency = eventingOptions.MaxConcurrency;
            }
        });

    public CoreApiBuilderOptions UseServiceBusEventing(string connectionString) =>
        WithCoreConfiguration(configure: coreConfig =>
        {
            coreConfig.Eventing.ProviderType = "ServiceBus";

            if (!string.IsNullOrWhiteSpace(value: connectionString))
            {
                coreConfig.Eventing.ServiceBus.ConnectionString = connectionString;
            }
        });

    public CoreApiBuilderOptions AddSecurityApi(
        Action<IServiceCollection, SecurityConfiguration> configure = null)
    {
        string rootPath = "Api/Security";

        cCoder.Security.IServiceCollectionExtensions.AddSecurityApi(services: services, configAction: (securityServices, securityConfig) =>
        {
            securityConfig.RootPath = coreConfiguration?.Security.RootPath ?? rootPath;
            securityConfig.ConnectionString =
                coreConfiguration?.Security.ConnectionString ?? string.Empty;

            securityConfig.UseAESHMMACPasswordEncryption(
services: securityServices, decryptionKey: coreConfiguration?.Security.DecryptionKey ?? string.Empty);

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
            && !string.IsNullOrWhiteSpace(value: coreConfiguration?.ContentManagement.ConnectionString))
        {
            domains.Connection = coreConfiguration.ContentManagement.ConnectionString;
        }

        if (string.IsNullOrWhiteSpace(value: domains.Connection))
        {
            throw new InvalidOperationException(
                "CoreDomainsConfig.Connection must be provided when adding the core business domains or available via core configuration.");
        }

        cCoder.Data.IServiceCollectionExtensions.AddData(
            services: services,
            configuration: new cCoder.Data.Models.DataConfiguration
            {
                ConnectionString = domains.Connection,
            });

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
        return ConfigureDomainsWith(configuration: configuration);
    }

    public CoreApiBuilderOptions ConfigureDomainsWith(CoreConfiguration configuration)
    {
        configuration ??= new CoreConfiguration();
        coreConfiguration = configuration;
        services.AddSingleton(implementationInstance: configuration);

        services.AddSecurityApi(configuration.Security);
        AddAppSecurityApi(configuration.AppSecurity);
        AddContentManagementApi(configuration.ContentManagement);
        AddDocumentManagementApi(configuration.DocumentManagement);
        AddLoggingApi(configuration.Logging);
        AddMailApi(configuration.Mail);
        AddWorkflowApi(configuration.Workflow);
        UseLegacyCoreApi();
        UseConfiguredExternalEventing(configuration: configuration);
        WithEventProviders(eventProviders: configuration.Eventing.EventProviders ?? []);

        return this;
    }

    public CoreApiBuilderOptions UseAll(Action<CoreConfiguration> configure) =>
        ConfigureDomainsWith(configure: configure);

    public CoreApiBuilderOptions AddAppSecurityApi(
        Action<AppSecurityConfiguration> configure = null)
    {
        AppSecurityConfiguration domain =
            coreConfiguration?.AppSecurity ?? new AppSecurityConfiguration();
        configure?.Invoke(obj: domain);
        return AddAppSecurityApi(domain);
    }

    internal CoreApiBuilderOptions AddAppSecurityApi(AppSecurityConfiguration domain)
    {
        ApplyDomainRouteMode(configuration: domain, domainName: "AppSecurity");
        domain.IncludeLegacyCoreContext = false;
        services.AddAppSecurityWeb(domain, new ODataConventionModelBuilder());

        RegisterDomainContext(
routePath: domain.RootPath, includeLegacyCoreContext: domain.IncludeLegacyCoreContext, configureModel: static builder => builder.ConfigureAppSecurityApiModel());

        return this;
    }

    public CoreApiBuilderOptions AddContentManagementApi(
        Action<ContentManagementConfiguration> configure = null)
    {
        ContentManagementConfiguration domain =
            coreConfiguration?.ContentManagement ?? new ContentManagementConfiguration();
        configure?.Invoke(obj: domain);
        return AddContentManagementApi(domain);
    }

    internal CoreApiBuilderOptions AddContentManagementApi(ContentManagementConfiguration domain)
    {
        ApplyDomainRouteMode(configuration: domain, domainName: "ContentManagement");
        domain.IncludeLegacyCoreContext = false;
        services.AddContentManagementWeb(domain, new ODataConventionModelBuilder());

        RegisterDomainContext(
routePath: domain.RootPath, includeLegacyCoreContext: domain.IncludeLegacyCoreContext, configureModel: static builder => builder.ConfigureContentManagementApiModel());

        return this;
    }

    public CoreApiBuilderOptions AddDocumentManagementApi(
        Action<DocumentManagementConfiguration> configure = null)
    {
        DocumentManagementConfiguration domain =
            coreConfiguration?.DocumentManagement ?? new DocumentManagementConfiguration();
        configure?.Invoke(obj: domain);
        return AddDocumentManagementApi(domain);
    }

    internal CoreApiBuilderOptions AddDocumentManagementApi(DocumentManagementConfiguration domain)
    {
        ApplyDomainRouteMode(configuration: domain, domainName: "DocumentManagement");
        domain.IncludeLegacyCoreContext = false;
        services.AddDocumentManagementWeb(domain, new ODataConventionModelBuilder());

        RegisterDomainContext(
routePath: domain.RootPath, includeLegacyCoreContext: domain.IncludeLegacyCoreContext, configureModel: static builder => builder.ConfigureDocumentManagementApiModel());

        return this;
    }

    public CoreApiBuilderOptions AddLoggingApi(
        Action<LoggingConfiguration> configure = null)
    {
        LoggingConfiguration domain = coreConfiguration?.Logging ?? new LoggingConfiguration();
        configure?.Invoke(obj: domain);
        return AddLoggingApi(domain);
    }

    internal CoreApiBuilderOptions AddLoggingApi(LoggingConfiguration domain)
    {
        ApplyDomainRouteMode(configuration: domain, domainName: "Logging");
        domain.IncludeLegacyCoreContext = false;
        services.AddLoggingWeb(domain, new ODataConventionModelBuilder());

        RegisterDomainContext(
routePath: domain.RootPath, includeLegacyCoreContext: domain.IncludeLegacyCoreContext, configureModel: static builder => builder.ConfigureLoggingApiModel());

        return this;
    }

    public CoreApiBuilderOptions AddMailApi(
        Action<MailConfiguration> configure = null)
    {
        MailConfiguration domain = coreConfiguration?.Mail ?? new MailConfiguration();
        configure?.Invoke(obj: domain);
        return AddMailApi(domain);
    }

    internal CoreApiBuilderOptions AddMailApi(MailConfiguration domain)
    {
        ApplyDomainRouteMode(configuration: domain, domainName: "Mail");
        domain.IncludeLegacyCoreContext = false;
        services.AddMailWeb(domain, new ODataConventionModelBuilder());

        RegisterDomainContext(
routePath: domain.RootPath, includeLegacyCoreContext: domain.IncludeLegacyCoreContext, configureModel: static builder => builder.ConfigureMailApiModel());

        return this;
    }

    public CoreApiBuilderOptions AddWorkflowApi(
        Action<WorkflowConfiguration> configure = null)
    {
        WorkflowConfiguration domain = coreConfiguration?.Workflow ?? new WorkflowConfiguration();
        configure?.Invoke(obj: domain);
        return AddWorkflowApi(domain);
    }

    internal CoreApiBuilderOptions AddWorkflowApi(WorkflowConfiguration domain)
    {
        ApplyDomainRouteMode(configuration: domain, domainName: "Workflow");
        domain.IncludeLegacyCoreContext = false;
        services.AddWorkflowWeb(domain, new ODataConventionModelBuilder());

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

    internal void ConfigureEventing(EventingConfiguration configuration)
    {
        coreConfiguration ??= new CoreConfiguration();
        coreConfiguration.Eventing = configuration;
        UseConfiguredExternalEventing(configuration: coreConfiguration);
        WithEventProviders(eventProviders: configuration.EventProviders ?? []);
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

    private void ApplyHttpEventing()
    {
        if (coreConfiguration is null
            || !IsHttpEventProvider(configuration: coreConfiguration.Eventing)
            || string.IsNullOrWhiteSpace(coreConfiguration.Eventing.Http.HubUrl))
        {
            return;
        }

        services.AddHttpEventing(configure: options =>
        {
            options.HubUrl = coreConfiguration.Eventing.Http.HubUrl;
            options.MaxConcurrency = coreConfiguration.Eventing.Http.MaxConcurrency;
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });
    }

    private void ApplyServiceBusEventing()
    {
        if (coreConfiguration is null
            || !IsServiceBusEventProvider(configuration: coreConfiguration.Eventing)
            || string.IsNullOrWhiteSpace(coreConfiguration.Eventing.ServiceBus.ConnectionString))
        {
            return;
        }

        services.AddTransient<ServiceBusAppDeleteForwardingService>();
        services.AddTransient<ServiceBusFolderDeleteForwardingService>();

        services.AddTransient<
            IServiceBusEventingBroker,
            ServiceBusEventingDependency>();

        services.AddTransient<
            IServiceBusAppDeleteForwardingBroker,
            ServiceBusAppDeleteForwardingBroker>();

        services.AddTransient<
            IServiceBusFolderDeleteForwardingBroker,
            ServiceBusFolderDeleteForwardingBroker>();

        services.AddAzureServiceBusEventing(configure: options =>
        {
            options.ConnectionString = coreConfiguration.Eventing.ServiceBus.ConnectionString;
            options.MaxConcurrency = coreConfiguration.Eventing.ServiceBus.MaxConcurrency;
        });
    }

    private void UseConfiguredExternalEventing(CoreConfiguration configuration)
    {
        if (IsServiceBusEventProvider(configuration: configuration.Eventing))
        {
            UseServiceBusEventing(
                connectionString: configuration.Eventing.ServiceBus.ConnectionString);
            return;
        }

        if (IsHttpEventProvider(configuration: configuration.Eventing))
        {
            UseHttpEventing(
                hubUrl: configuration.Eventing.Http.HubUrl,
                configure: options =>
                    options.MaxConcurrency = configuration.Eventing.Http.MaxConcurrency);
        }
    }

    private static bool IsHttpEventProvider(EventingConfiguration configuration) =>
        string.Equals(
            a: configuration.ProviderType,
            b: "Http",
            comparisonType: StringComparison.OrdinalIgnoreCase);

    private static bool IsServiceBusEventProvider(EventingConfiguration configuration) =>
        string.Equals(
            a: configuration.ProviderType,
            b: "ServiceBus",
            comparisonType: StringComparison.OrdinalIgnoreCase);

    private CoreConfiguration EnsureCoreConfiguration() =>
        coreConfiguration ??= new CoreConfiguration();


}
