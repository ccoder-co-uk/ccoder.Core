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

            services.AddTransient<ICoreService<Role>, RoleService>();
            services.AddTransient<ICoreService<UserRole>, UserRoleService>();
            services.AddTransient<ICoreService<User>, UserService>();

            setupAction(config);
        }

        internal static void AddCoreServices(this IServiceCollection services)
        {
            // CMS services
            services.AddTransient<IAppService, AppService>();
            services.AddTransient<ICoreService<AppCulture>, AppCultureService>();
            services.AddTransient<ICommonObjectService, CommonObjectService>();
            services.AddTransient<IComponentService, ComponentService>();
            services.AddTransient<ICoreService<Content>, ContentService>();
            services.AddTransient<ICoreService<Culture>, CultureService>();
            services.AddTransient<ICoreService<Layout>, LayoutService>();
            services.AddTransient<ICoreService<PackageItem>, PackageItemService>();
            services.AddTransient<IPackageService, PackageService>();
            services.AddTransient<IPageService, PageService>();
            services.AddTransient<ICoreService<PageInfo>, PageInfoService>();
            services.AddTransient<IResourceService, ResourceService>();
            services.AddTransient<IScheduledTaskService, ScheduledTaskService>();
            services.AddTransient<IScriptService, ScriptService>();
            services.AddTransient<ICoreService<Submission>, SubmissionService>();
            services.AddTransient<ITemplateService, TemplateService>();

            // Mail services
            services.AddTransient<IQueuedEmailService, QueuedEmailService>();
            services.AddTransient<ICoreService<SentEmail>, SentEmailService>();

            // Security services
            services.AddTransient<ICMSUserRegistrationOrchestrationService, CMSUserRegistrationOrchestrationService>();
            services.AddTransient<IUserRoleService, UserRoleService>();
            services.AddTransient<ICoreService<PageRole>, PageRoleService>();
            services.AddTransient<ICoreService<Privilege>, PrivilegeService>();

            //planning services
            services.AddTransient<ICoreService<CalendarEvent>, CalendarEventService>();
            services.AddTransient<ICoreService<Calendar>, CalendarService>();
            services.AddTransient<ICoreService<BackgroundJob>, BackgroundJobService>();

            // workflow services
            services.AddTransient<ICoreService<BusinessProcess>, BusinessProcessService>();
            services.AddTransient<IFlowDefinitionService, FlowDefinitionService>();
            services.AddTransient<ICoreService<FlowInstanceData>, FlowInstanceDataService>();
            services.AddTransient<ICoreService<WorkflowEvent>, WorkflowEventService>();
            
            // DMS Services.
            services.AddTransient<ICoreService<FileContent>, FileContentService>();
            services.AddTransient<ICoreService<FolderRole>, FolderRoleService>();
        }

        internal static void AddCorePackaging(this IServiceCollection services)
        {
            // installers
            services.AddTransient<IPackageInstaller, CorePackageInstaller>();

            // importers 
            services.AddTransient<IPackageItemImporter, BusinessProcessImporter>();
            services.AddTransient<IPackageItemImporter, ComponentImporter>();
            services.AddTransient<IPackageItemImporter, FlowDefinitionImporter>();
            services.AddTransient<IPackageItemImporter, FolderRoleImporter>();
            services.AddTransient<IPackageItemImporter, LayoutImporter>();
            services.AddTransient<IPackageItemImporter, PageImporter>();
            services.AddTransient<IPackageItemImporter, PageRoleImporter>();
            services.AddTransient<IPackageItemImporter, ResourceImporter>();
            services.AddTransient<IPackageItemImporter, RoleImporter>();
            services.AddTransient<IPackageItemImporter, ScriptImporter>();
            services.AddTransient<IPackageItemImporter, TemplateImporter>();
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

            services.AddTransient<IResourceProvider, CoreResourceProvider>();
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
            services.AddTransient(ctx => ctx.GetService<IHttpContextAccessor>()?.HttpContext);
            services.AddTransient(ctx => ctx.GetService<HttpContext>()?.Request);

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