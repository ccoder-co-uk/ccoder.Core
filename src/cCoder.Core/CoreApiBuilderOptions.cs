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
using cCoder.Security.Api;
using cCoder.Security.Data.EF.MSSQL;
using cCoder.Security.Objects;
using cCoder.Core.Api;
using cCoder.Workflow;
using cCoder.Workflow.Models;
using EventLibrary.Models;
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
    private IConfiguration configuration;
    private bool applied;

    public CoreApiBuilderOptions(IServiceCollection services) =>
        this.services = services;

    public CoreApiBuilderOptions UseConfiguration(IConfiguration configuration)
    {
        this.configuration = configuration;

        DataConfig config = new();
        configuration.Bind(config);

        services.AddSingleton(config);
        services.AddSingleton(CreateContentManagementRuntimeConfig(config));
        services.AddSingleton(CreateMailRuntimeConfig(config));

        return this;
    }

    public CoreApiBuilderOptions UseDefaultBaseline(
        IConfiguration configuration)
    {
        UseConfiguration(configuration);
        services.AddCoreApiEventing(configuration, eventProviders);

        AddSecurityApi((securityServices, securityConfig) =>
        {
            securityConfig.RootPath = "Api/Security";
            securityConfig.AddMSSQLModelProvider(
                securityServices,
                configuration.GetConnectionString("SSO"));
            securityConfig.UseAESHMMACPasswordEncryption(
                securityServices,
                configuration.GetSection("settings")["DecryptionKey"]);
        });

        AddAllDomains(domains =>
        {
            domains.Connection = configuration.GetConnectionString("Core");
            domains.RootPath = "Api";
            domains.SplitDomains = true;
            domains.IncludeLegacyCoreContext = true;
        });

        RegisterContext("Api/Core", static builder => builder.ConfigureCoreAggregateApiModel());

        return this;
    }

    public CoreApiBuilderOptions WithEventProviders(params EventProvider[] eventProviders)
    {
        this.eventProviders.AddRange((eventProviders ?? []).Where(provider => provider is not null));
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

        if (string.IsNullOrWhiteSpace(domains.Connection))
        {
            throw new InvalidOperationException(
                "CoreDomainsConfig.Connection must be provided when adding the core business domains.");
        }

        cCoder.Data.IServiceCollectionExtensions.AddCoreData(services, domains.Connection);

        AddAppSecurityApi((_, domain) => ConfigureDomainRouting(domain, "AppSecurity", domains));
        AddContentManagementApi((_, domain) => ConfigureDomainRouting(domain, "ContentManagement", domains));
        AddDocumentManagementApi((_, domain) => ConfigureDomainRouting(domain, "DocumentManagement", domains));
        AddLoggingApi((_, domain) => ConfigureDomainRouting(domain, "Logging", domains));
        AddMailApi((_, domain) => ConfigureDomainRouting(domain, "Mail", domains));
        AddSchedulingApi((_, domain) => ConfigureDomainRouting(domain, "Scheduling", domains));
        AddWorkflowApi((_, domain) => ConfigureDomainRouting(domain, "Workflow", domains));

        return this;
    }

    public CoreApiBuilderOptions AddAppSecurityApi(
        Action<IServiceCollection, AppSecurityConfiguration> configure = null)
    {
        AppSecurityConfiguration domain = services.AddAppSecurity(configure ?? ((_, _) => { }));
        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureAppSecurityApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddContentManagementApi(
        Action<IServiceCollection, ContentManagementConfiguration> configure = null)
    {
        ContentManagementConfiguration domain =
            services.AddContentManagement(configure ?? ((_, _) => { }));
        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureContentManagementApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddDocumentManagementApi(
        Action<IServiceCollection, DocumentManagementConfiguration> configure = null)
    {
        DocumentManagementConfiguration domain =
            services.AddDocumentManagement(configure ?? ((_, _) => { }));
        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureDocumentManagementApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddLoggingApi(
        Action<IServiceCollection, LoggingConfiguration> configure = null)
    {
        LoggingConfiguration domain = services.AddLogging(configure ?? ((_, _) => { }));
        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureLoggingApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddMailApi(
        Action<IServiceCollection, MailConfiguration> configure = null)
    {
        MailConfiguration domain = services.AddMail(configure ?? ((_, _) => { }));
        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureMailApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddSchedulingApi(
        Action<IServiceCollection, SchedulingConfiguration> configure = null)
    {
        SchedulingConfiguration domain = services.AddScheduling(configure ?? ((_, _) => { }));
        RegisterDomainContext(
            domain.RootPath,
            domain.IncludeLegacyCoreContext,
            static builder => builder.ConfigureSchedulingApiModel());
        return this;
    }

    public CoreApiBuilderOptions AddWorkflowApi(
        Action<IServiceCollection, WorkflowConfiguration> configure = null)
    {
        WorkflowConfiguration domain = services.AddWorkflow(configure ?? ((_, _) => { }));
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
}
