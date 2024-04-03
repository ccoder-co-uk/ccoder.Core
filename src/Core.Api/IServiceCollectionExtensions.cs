using Core.Api.Formatters;
using Core.Api.OData;
using Core.Objects;
using Core.Objects.Dtos.Metadata;
using Core.Objects.Entities.CMS;
using Core.Objects.Entities.DMS;
using Core.Objects.Entities.Mail;
using Core.Objects.Entities.Packaging;
using Core.Objects.Entities.Planning;
using Core.Objects.Entities.Security;
using Core.Objects.Entities.Workflow;
using Core.Packaging;
using Core.Packaging.Importers;
using Core.Services;
using Core.Services.CMS;
using Core.Services.DMS;
using Core.Services.Orchestrations;
using Core.Services.Orchestrations.Interfaces;
using Core.Services.Security;
using Core.Services.Workflow;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Core.Api
{
    public static partial class IServiceCollectionExtensions
    {
        public static void AddCore(this IServiceCollection services, Action<CoreBuilderOptions> setupAction)
        {
            var config = new CoreBuilderOptions(services);

            services.AddScoped<ICoreService<Role>, RoleService>();
            services.AddScoped<ICoreService<UserRole>, UserRoleService>();
            services.AddScoped<ICoreService<User>, UserService>();

            setupAction(config);
        }

        internal static void AddCoreServices(this IServiceCollection services)
        {
            // CMS services
            services.AddScoped<IAppService, AppService>();
            services.AddScoped<ICoreService<AppCulture>, AppCultureService>();
            services.AddScoped<ICommonObjectService, CommonObjectService>();
            services.AddScoped<IComponentService, ComponentService>();
            services.AddScoped<ICoreService<Content>, ContentService>();
            services.AddScoped<ICoreService<Culture>, CultureService>();
            services.AddScoped<ICoreService<Layout>, LayoutService>();
            services.AddScoped<ICoreService<PackageItem>, PackageItemService>();
            services.AddScoped<IPackageService, PackageService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<ICoreService<PageInfo>, PageInfoService>();
            services.AddScoped<IResourceService, ResourceService>();
            services.AddScoped<IScheduledTaskService, ScheduledTaskService>();
            services.AddScoped<IScriptService, ScriptService>();
            services.AddScoped<ICoreService<Submission>, SubmissionService>();
            services.AddScoped<ITemplateService, TemplateService>();

            // Mail services
            services.AddScoped<IQueuedEmailService, QueuedEmailService>();
            services.AddScoped<ICoreService<SentEmail>, SentEmailService>();

            // Security services
            services.AddScoped<ICMSUserRegistrationOrchestrationService, CMSUserRegistrationOrchestrationService>();
            services.AddScoped<IUserRoleService, UserRoleService>();
            services.AddScoped<ICoreService<PageRole>, PageRoleService>();
            services.AddScoped<ICoreService<Privilege>, PrivilegeService>();

            //planning services
            services.AddScoped<ICoreService<CalendarEvent>, CalendarEventService>();
            services.AddScoped<ICoreService<Calendar>, CalendarService>();
            services.AddScoped<ICoreService<BackgroundJob>, BackgroundJobService>();

            // workflow services
            services.AddScoped<ICoreService<BusinessProcess>, BusinessProcessService>();
            services.AddScoped<IFlowDefinitionService, FlowDefinitionService>();
            services.AddScoped<ICoreService<FlowInstanceData>, FlowInstanceDataService>();
            services.AddScoped<ICoreService<WorkflowEvent>, WorkflowEventService>();
            
            // DMS Services.
            services.AddScoped<ICoreService<FileContent>, FileContentService>();
            services.AddScoped<ICoreService<FolderRole>, FolderRoleService>();
        }

        internal static void AddCorePackaging(this IServiceCollection services)
        {
            // installers
            services.AddScoped<IPackageInstaller, CorePackageInstaller>();

            // importers 
            services.AddScoped<IPackageItemImporter, BusinessProcessImporter>();
            services.AddScoped<IPackageItemImporter, ComponentImporter>();
            services.AddScoped<IPackageItemImporter, FlowDefinitionImporter>();
            services.AddScoped<IPackageItemImporter, FolderRoleImporter>();
            services.AddScoped<IPackageItemImporter, LayoutImporter>();
            services.AddScoped<IPackageItemImporter, PageImporter>();
            services.AddScoped<IPackageItemImporter, PageRoleImporter>();
            services.AddScoped<IPackageItemImporter, ResourceImporter>();
            services.AddScoped<IPackageItemImporter, RoleImporter>();
            services.AddScoped<IPackageItemImporter, ScriptImporter>();
            services.AddScoped<IPackageItemImporter, TemplateImporter>();
        }

        internal static void AddCaches(
            this IServiceCollection services, 
            IEnumerable<MetadataContainerSet> additionalMetadata)
        {
            services.AddSingleton<ICommonObjectCache, CommonObjectCache>();

            services.AddSingleton<IMetadataCache>(ctx => 
                new MetadataCache(
                    MetadataHelper.MetaForEverything().Union(additionalMetadata), 
                    ctx.GetRequiredService<ICommonObjectCache>()));

            services.AddScoped<IResourceProvider, CoreResourceProvider>();
        }

        internal static void AddCoreApi(this IServiceCollection services)
        {
            services.AddAspNet();

            //TODO break this out in to sub models
            services.AddControllers()
                .AddOData(opt =>
                {
                    opt.RouteOptions.EnableQualifiedOperationCall = false;
                    opt.EnableAttributeRouting = true;
                    _ = opt.Expand().Count().Filter().Select().OrderBy().SetMaxTop(1000);
                    opt.AddRouteComponents($"/Api/Core", new CoreModelBuilder().Build().EDMModel);
                });
        }

        static void AddAspNet(this IServiceCollection services)
        {
            services.AddRouting();
            services.AddResponseCompression();

            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddScoped(ctx => ctx.GetService<IHttpContextAccessor>()?.HttpContext);
            services.AddScoped(ctx => ctx.GetService<HttpContext>()?.Request);

            services.AddSession();
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromMinutes(60);
            });

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                options.OutputFormatters.Add(new XmlFormatter());
                options.OutputFormatters.Add(new CsvFormatter());
                options.OutputFormatters.Add(new ExcelFormatter());
            });
            services.AddRazorPages();

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
            });

            services.AddEndpointsApiExplorer();
            services.AddSignalR();
        }
    }
}