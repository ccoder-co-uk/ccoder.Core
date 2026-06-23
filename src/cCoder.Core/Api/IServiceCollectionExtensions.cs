using cCoder.Core.Api.Formatters;
using cCoder.Core.Brokers.AppSecurity;
using cCoder.Core.Brokers.ContentManagement;
using cCoder.Core.Brokers.DocumentManagement;
using cCoder.Core.Brokers.Mail;
using cCoder.Core.Brokers.Packaging;
using cCoder.Core.Brokers.Planning;
using cCoder.Core.Brokers.Workflow;
using cCoder.Core.Services.Foundations.AppSecurity;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Foundations.DocumentManagement;
using cCoder.Core.Services.Foundations.Mail;
using cCoder.Core.Services.Foundations.Planning;
using cCoder.Core.Services.Foundations.Workflow;
using cCoder.Core.Services.Orchestrations;
using cCoder.Core.Cors;
using cCoder.Core.Models;
using cCoder.Packaging;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;


namespace cCoder.Core.Api;

internal static class IServiceCollectionExtensions
{
    public static void AddAspNet(this IServiceCollection services)
    {
        services.AddRouting();
        services.AddResponseCompression();

        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddScoped(typeof(HttpContext), ctx => CreateHttpContext(ctx.GetService<IHttpContextAccessor>()?.HttpContext));
        services.AddScoped(typeof(HttpRequest), ctx => ctx.GetRequiredService<HttpContext>().Request);
        services.AddScoped(typeof(ISession), ctx =>
        {
            HttpContext httpContext = ctx.GetRequiredService<HttpContext>();
            return httpContext.Features.Get<ISessionFeature>()?.Session ?? NoOpSession.Instance;
        });

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
            options.Limits.MaxRequestBodySize = int.MaxValue;
        });

        services.AddEndpointsApiExplorer();
        services.AddSignalR();
    }

    public static IServiceCollection AddCoreApi(
        this IServiceCollection services,
        IEnumerable<CoreApiRouteDefinition> routeDefinitions = null
    )
    {
        AddAspNet(services);
        services.AddTransient<IContentManagementAppBroker, ContentManagementAppBroker>();
        services.AddTransient<IAppSecurityAppBroker, AppSecurityAppBroker>();
        services.AddTransient<IPlanningAppBroker, PlanningAppBroker>();
        services.AddTransient<IDocumentManagementAppBroker, DocumentManagementAppBroker>();
        services.AddTransient<IWorkflowAppBroker, WorkflowAppBroker>();
        services.AddTransient<IMailAppBroker, MailAppBroker>();
        services.AddTransient<IMailManagerBroker, MailManagerBroker>();
        services.AddTransient<IContentManagementAppService, ContentManagementAppService>();
        services.AddTransient<IAppSecurityAppService, AppSecurityAppService>();
        services.AddTransient<IPlanningAppService, PlanningAppService>();
        services.AddTransient<IDocumentManagementAppService, DocumentManagementAppService>();
        services.AddTransient<IWorkflowAppService, WorkflowAppService>();
        services.AddTransient<IMailAppService, MailAppService>();
        services.AddTransient<IMailManagerService, MailManagerService>();
        services.AddTransient<IAppOrchestrationService, AppOrchestrationService>();
        services.AddTransient<ITemplatedEmailOrchestrationService, TemplatedEmailOrchestrationService>();
        services.AddTransient<ICMSUserRegistrationOrchestrationService, CMSUserRegistrationOrchestrationService>();
        services.AddSingleton<ICoreAllowedOriginStore, CoreAllowedOriginStore>();
        services.AddSingleton<ICoreEventHandlers, CoreAllowedOriginEventHandlers>();
        services.AddTransient<IUserRegistrationOrchestrationService, UserRegistrationOrchestrationService>();
        services.AddTransient<IUserPasswordOrchestrationService, UserPasswordOrchestrationService>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IAppDomainProvider, AppDomainProvider>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IAppSecurityPackageManagerBroker, AppSecurityPackageManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IContentManagementPackageManagerBroker, ContentManagementPackageManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IDocumentManagementPackageManagerBroker, DocumentManagementPackageManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.ISchedulingPackageManagerBroker, SchedulingPackageManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IWorkflowPackageManagerBroker, WorkflowPackageManagerBroker>();
        services.AddPackaging();

        DefaultODataBatchHandler batchHandler = new();
        CoreApiRouteDefinition[] definitions = (routeDefinitions ?? [])
            .Where(route =>
                route is not null
                && (string.Equals(route.Name, "Core", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(route.RoutePath, "Api/Core", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(route.Name, "Security", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(route.RoutePath, "Api/Security", StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        services.AddControllers().AddOData(opt =>
        {
            opt.RouteOptions.EnableQualifiedOperationCall = false;
            opt.EnableAttributeRouting = true;
            opt.RouteOptions.EnableKeyAsSegment = false;
            opt.Expand()
                .Count()
                .Filter()
                .Select()
                .OrderBy()
                .SetMaxTop(1000);

            foreach (CoreApiRouteDefinition routeDefinition in definitions)
            {
                _ = opt.AddRouteComponents(
                    routeDefinition.RoutePath,
                    routeDefinition.RouteModel,
                    batchHandler);
            }
        });

        return services;
    }

    private static HttpContext CreateHttpContext(HttpContext httpContext)
    {
        if (httpContext is not null)
            return httpContext;

        DefaultHttpContext fallbackContext = new();
        fallbackContext.Features.Set<ISessionFeature>(new NoOpSessionFeature());
        return fallbackContext;
    }
}




