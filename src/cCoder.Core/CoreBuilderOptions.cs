using cCoder.ContentManagement;
using cCoder.DocumentManagement;
using cCoder.Mail;
using cCoder.Packaging;
using cCoder.Scheduling;
using cCoder.AppSecurity;
using cCoder.Workflow;
using cCoder.Core.Api;
using Microsoft.OData.Edm;


namespace cCoder.Core;

public class CoreBuilderOptions
{
    private readonly IServiceCollection services;

    public CoreBuilderOptions(IServiceCollection services) => this.services = services;

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
        services.AddContentManagementInfrastructure(map);
        return this;
    }

    public CoreBuilderOptions UseDocumentManagement()
    {
        services.AddDocumentManagement();
        return this;
    }

    public CoreBuilderOptions UseApi(IDictionary<string, IEdmModel> routeModels = null)
    {
        services.AddCoreApi(routeModels: routeModels);
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






