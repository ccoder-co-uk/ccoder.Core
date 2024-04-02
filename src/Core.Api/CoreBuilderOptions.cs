using Core.Data;
using Core.Objects;
using Core.Objects.Dtos.Metadata;
using Core.Objects.Entities.Security;
using Core.Services;
using Core.Services.DMS;

namespace Core.Api
{
    public class CoreBuilderOptions
    {
        private readonly IServiceCollection services;

        public CoreBuilderOptions(IServiceCollection services) =>
            this.services = services;

        public CoreBuilderOptions UseMSSQLProvider(string connectionString)
        {
            //services.AddDbContext<CoreDataContext>();
            services.AddTransient<ICoreDataContext, CoreDataContext>();
            services.AddDbContextFactory<CoreDataContext>(lifetime: ServiceLifetime.Transient);

            return this;
        }

        public CoreBuilderOptions UseContentManagement(
            IEnumerable<MetadataContainerSet> additionalMetadata = null, bool servicesOnly = false)
        {
            services.AddCoreServices();
            services.AddCorePackaging();
            services.AddCaches(additionalMetadata ?? Array.Empty<MetadataContainerSet>());

            if(!servicesOnly)
                services.AddCoreApi();

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
}