using cCoder.Core.Api.Formatters;
using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Objects.Entities.Mail;
using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Objects.Entities.Planning;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Services;
using cCoder.Core.Services.CMS;
using cCoder.Core.Services.DMS;
using cCoder.Core.Services.Mail;
using cCoder.Core.Services.Orchestrations;
using cCoder.Core.Services.Orchestrations.Interfaces;
using cCoder.Core.Services.Packaging;
using cCoder.Core.Services.Packaging.Importers;
using cCoder.Core.Services.Planning;
using cCoder.Core.Services.Security;
using cCoder.Core.Services.Workflow;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OData.Edm;

namespace cCoder.Core.Api;

public static partial class IServiceCollectionExtensions
{
    public static void AddCore(this IServiceCollection services, Action<CoreBuilderOptions> setupAction)
    {
        CoreBuilderOptions config = new(services);

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
        services.AddScoped<ICoreService<MailServer>, MailServerService>();

        // Security services
        services.AddScoped<ICMSUserRegistrationOrchestrationService, CMSUserRegistrationOrchestrationService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<ICoreService<PageRole>, PageRoleService>();
        services.AddScoped<ICoreService<Privilege>, PrivilegeService>();

        //planning services
        services.AddScoped<ICoreService<CalendarEvent>, CalendarEventService>();
        services.AddScoped<ICoreService<Calendar>, CalendarService>();

        // workflow services
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
        services.AddScoped<IPackageItemImporter, CalendarImporter>();
        services.AddScoped<IPackageItemImporter, CalendarEventImporter>();
    }

    internal static void AddCaches(
        this IServiceCollection services,
        IDictionary<string, IEdmModel> map)
    {
        services.AddSingleton<ICommonObjectCache, CommonObjectCache>();

        services.AddSingleton<IMetadataCache>(ctx => 
            new MetadataCache(
                MetadataHelper.MetaForEverything(map), 
                ctx.GetRequiredService<ICommonObjectCache>()));

        services.AddScoped<IResourceProvider, CoreResourceProvider>();
    }

    internal static void AddAspNet(this IServiceCollection services)
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