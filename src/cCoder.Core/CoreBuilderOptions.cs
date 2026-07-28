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
        services.AddSingleton(implementationInstance: coreConfiguration);

        return this;
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

    public CoreBuilderOptions WithSecurity(
        string connectionString,
        string decryptionKey)
    {
        cCoder.Security.IServiceCollectionExtensions.AddSecurity(services: services, configAction: (securityServices, securityConfig) =>
        {
            securityConfig.ConnectionString = connectionString ?? string.Empty;

            securityConfig.UseAESHMMACPasswordEncryption(
services: securityServices, decryptionKey: decryptionKey ?? string.Empty);
        });

        return WithCoreConfiguration(configure: coreConfig =>
        {
            if (!string.IsNullOrWhiteSpace(value: connectionString))
            {
                coreConfig.Security.ConnectionString = connectionString;
            }

            if (!string.IsNullOrWhiteSpace(value: decryptionKey))
            {
                coreConfig.Security.DecryptionKey = decryptionKey;
            }
        });
    }

    public CoreBuilderOptions UseHttpEventing() =>
        WithCoreConfiguration(configure: coreConfig => coreConfig.Eventing.ProviderType = "Http");

    public CoreBuilderOptions UseServiceBusEventing() =>
        WithCoreConfiguration(configure: coreConfig => coreConfig.Eventing.ProviderType = "ServiceBus");

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

    public CoreBuilderOptions UseContentManagement(
        Action<ContentManagementConfiguration> configure = null,
        IDictionary<string, IEdmModel> map = null,
        bool servicesOnly = false
    )
    {
        ContentManagementConfiguration configuration =
            coreConfiguration?.ContentManagement ?? new ContentManagementConfiguration();
        configure?.Invoke(obj: configuration);
        services.AddContentManagementHostedServices(configuration);

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
        DocumentManagementConfiguration configuration =
            coreConfiguration?.DocumentManagement ?? new DocumentManagementConfiguration();
        configure?.Invoke(obj: configuration);
        services.AddDocumentManagementHostedServices(configuration);

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
        MailConfiguration configuration = coreConfiguration?.Mail ?? new MailConfiguration();
        configure?.Invoke(obj: configuration);
        services.AddMailHostedServices(configuration);

        return this;
    }

    public CoreBuilderOptions UseWorkflow(Action<WorkflowConfiguration> configure = null)
    {
        WorkflowConfiguration configuration = coreConfiguration?.Workflow ?? new WorkflowConfiguration();
        configure?.Invoke(obj: configuration);
        services.AddWorkflowHostedServices(configuration);

        return this;
    }

    public CoreBuilderOptions UseAppSecurity(Action<AppSecurityConfiguration> configure = null)
    {
        AppSecurityConfiguration configuration =
            coreConfiguration?.AppSecurity ?? new AppSecurityConfiguration();
        configure?.Invoke(obj: configuration);
        services.AddAppSecurityHostedServices(configuration);

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
        LoggingConfiguration configuration = coreConfiguration?.Logging ?? new LoggingConfiguration();
        configure?.Invoke(obj: configuration);
        services.AddLoggingHostedServices(configuration);

        return this;
    }

    public CoreBuilderOptions AuthorizeUsersWith()
    {
        return this;
    }

    public CoreBuilderOptions ConfigureDomainsWith(Action<CoreConfiguration> configure)
    {
        CoreConfiguration configuration = new();
        configure?.Invoke(obj: configuration);
        return ConfigureDomainsWith(configuration: configuration);
    }

    public CoreBuilderOptions ConfigureDomainsWith(CoreConfiguration configuration)
    {
        configuration ??= new CoreConfiguration();
        coreConfiguration = configuration;
        services.AddSingleton(implementationInstance: configuration);

        services.AddSecurityHostedServices(configuration.Security);
        services.AddAppSecurityHostedServices(configuration.AppSecurity);
        services.AddContentManagementHostedServices(configuration.ContentManagement);
        services.AddDocumentManagementHostedServices(configuration.DocumentManagement);
        services.AddLoggingHostedServices(configuration.Logging);
        services.AddMailHostedServices(configuration.Mail);
        services.AddWorkflowHostedServices(configuration.Workflow);

        services.AddScoped<ICoreAllowedOriginStore, CoreAllowedOriginStore>();
        services.TryAddTransient<IAppSecurityAppService, AppSecurityAppService>();
        services.TryAddTransient<IAppSecurityUserRoleService, AppSecurityUserRoleService>();
        services.TryAddTransient<
            IHostedServicesAppSecurityAppAddOrchestrationService,
            HostedServicesAppSecurityAppAddOrchestrationService>();
        services.TryAddTransient<IContentManagementAppBroker, ContentManagementAppBroker>();
        services.TryAddTransient<IHttpRequestBroker, HttpRequestBroker>();
        services.TryAddTransient<IContentManagementAppService, ContentManagementAppService>();
        services.TryAddTransient<IAllowedOriginStoreService, AllowedOriginStoreService>();
        services.TryAddTransient<IAllowedOriginStoreProcessingService, AllowedOriginStoreProcessingService>();

        UseConfiguredExternalEventing(configuration: configuration);
        WithEventProviders(eventProviders: configuration.Eventing.EventProviders ?? []);

        return this;
    }

    internal void ConfigureCoreServices(CoreConfiguration configuration)
    {
        coreConfiguration = configuration;

        services.AddScoped<ICoreAllowedOriginStore, CoreAllowedOriginStore>();
        services.TryAddTransient<IAppSecurityAppService, AppSecurityAppService>();
        services.TryAddTransient<IAppSecurityUserRoleService, AppSecurityUserRoleService>();
        services.TryAddTransient<
            IHostedServicesAppSecurityAppAddOrchestrationService,
            HostedServicesAppSecurityAppAddOrchestrationService>();
        services.TryAddTransient<IContentManagementAppBroker, ContentManagementAppBroker>();
        services.TryAddTransient<IHttpRequestBroker, HttpRequestBroker>();
        services.TryAddTransient<IContentManagementAppService, ContentManagementAppService>();
        services.TryAddTransient<IAllowedOriginStoreService, AllowedOriginStoreService>();
        services.TryAddTransient<IAllowedOriginStoreProcessingService, AllowedOriginStoreProcessingService>();

        UseConfiguredExternalEventing(configuration);
        WithEventProviders(configuration.Eventing.EventProviders ?? []);
    }

    public CoreBuilderOptions UseAll(Action<CoreConfiguration> configure) =>
        ConfigureDomainsWith(configure: configure);


    internal void Apply()
    {
        ApplySessionCacheFallback();
        ApplyHttpEventing();
        ApplyServiceBusEventing();
        services.AddCoreEventing(eventProviders: eventProviders);
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

        services.AddHttpEventingHostedServices(configure: options =>
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
            options.ConnectionString = coreConfiguration.Eventing.ServiceBus.ConnectionString;
            options.MaxConcurrency = coreConfiguration.Eventing.ServiceBus.MaxConcurrency;
        });
    }

    private void UseConfiguredExternalEventing(CoreConfiguration configuration)
    {
        if (IsServiceBusEventProvider(configuration: configuration.Eventing))
        {
            UseServiceBusEventing();
            return;
        }

        if (IsHttpEventProvider(configuration: configuration.Eventing))
        {
            UseHttpEventing();
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
