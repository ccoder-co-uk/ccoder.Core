using cCoder.Core.Data;
using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Services;
using cCoder.Core.Services.DMS;
using Microsoft.OData.Edm;

namespace cCoder.Core.Api;

public class CoreBuilderOptions
{
    private readonly IServiceCollection services;

    public CoreBuilderOptions(IServiceCollection services)
    {
        this.services = services;
    }

    public CoreBuilderOptions UseMSSQLProvider(string connectionString)
    {
        //services.AddDbContext<CoreDataContext>();
        services.AddScoped<ICoreDataContext, CoreDataContext>();

        services.AddDbContextFactory<CoreDataContext>(lifetime: ServiceLifetime.Scoped);

        return this;
    }

    public CoreBuilderOptions UseContentManagement(IDictionary<string, IEdmModel> map = null, bool servicesOnly = false)
    {
        services.AddCoreServices();
        services.AddCorePackaging();
        services.AddCaches(map ?? new Dictionary<string, IEdmModel>());

        if (!servicesOnly)
            services.AddAspNet();

        return this;
    }

    public CoreBuilderOptions UseDocumentManagement()
    {
        // DMS service
        services.AddTransient<IFileService, FileService>();
        services.AddTransient<IFolderService, FolderService>();
        services.AddTransient<ICoreService<FolderRole>, FolderRoleService>();

        return this;
    }

    public CoreBuilderOptions AuthorizeUsersWith(Func<IServiceProvider, string> authResolver)
    {
        services.AddTransient<ICoreAuthInfo>(ctx => new CoreAuthInfo { SSOUserId = authResolver(ctx) });
        return this;
    }
}

public class CoreAuthInfo : ICoreAuthInfo
{
    public string SSOUserId { get; internal set; }
}