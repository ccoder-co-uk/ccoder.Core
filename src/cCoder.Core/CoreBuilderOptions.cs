using cCoder.AppSecurity;
using cCoder.ContentManagement;
using cCoder.Core.Api;
using cCoder.Core.Models;
using cCoder.Data;
using cCoder.DocumentManagement;
using cCoder.Mail;
using cCoder.Packaging;
using cCoder.Scheduling;
using cCoder.Workflow;
using EventLibrary.Models;
using Microsoft.OData.Edm;


namespace cCoder.Core;

public partial class CoreBuilderOptions
{
    private readonly IServiceCollection services;
    private readonly List<EventProvider> eventProviders = [];
    private IConfiguration configuration;

    public CoreBuilderOptions(IServiceCollection services) => 
        this.services = services;

    public CoreBuilderOptions UseConfiguration(IConfiguration configuration)
    {
        this.configuration = configuration;

        Config config = new();
        configuration.Bind(config);
        services.AddSingleton(config);

        return this;
    }

    public CoreBuilderOptions UseDefaultBaseline(IConfiguration configuration)
    {
        UseConfiguration(configuration);
        services.AddCoreHostedEventing(configuration, eventProviders);
        ConfigureSecurityAndDataServices();
        ConfigureHostedDomainServices();
        cCoder.Core.Api.IServiceCollectionExtensions.AddAspNet(services);
        return this;
    }

    public CoreBuilderOptions WithEventProviders(params EventProvider[] eventProviders)
    {
        this.eventProviders.AddRange((eventProviders ?? []).Where(provider => provider is not null));
        return this;
    }

    public CoreBuilderOptions UseMSSQLProvider(string connectionString)
    {
        services.AddCoreDataAccess(connectionString);
        return this;
    }

    public CoreBuilderOptions UseContentManagement(
        IDictionary<string, IEdmModel> map = null,
        bool servicesOnly = false
    )
    {
        services.AddContentManagement();
        services.AddPackaging();
        return this;
    }

    public CoreBuilderOptions UseDocumentManagement()
    {
        services.AddDocumentManagement();
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

    public CoreBuilderOptions UseMail()
    {
        services.AddMail();
        return this;
    }

    public CoreBuilderOptions UseScheduling()
    {
        services.AddScheduling();
        return this;
    }

    public CoreBuilderOptions UseWorkflow()
    {
        services.AddWorkflow();
        return this;
    }

    public CoreBuilderOptions UseAppSecurity()
    {
        services.AddAppSecurity();
        return this;
    }

    public CoreBuilderOptions UseLogging()
    {
        cCoder.Logging.IServiceCollectionExtensions.AddLogging(services);
        return this;
    }

    public CoreBuilderOptions AuthorizeUsersWith()
    {
        services.AddCoreAuthInfo();
        return this;
    }
}






