using cCoder.Core.Exposures.Cors;
using cCoder.Core.Exposures.Formatters;
using cCoder.Core.Exposures;
using cCoder.Core.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    private static IServiceCollection AddCoreExposures(
        IServiceCollection services,
        IEnumerable<CoreApiRouteDefinition> routeDefinitions = null)
    {
        AddCoreAspNetExposures(services);
        AddCoreBrokers(services);
        AddCoreFoundationServices(services);
        AddCoreProcessingServices(services);
        AddCoreOrchestrationServices(services);
        services.AddScoped<ICoreAllowedOriginStore, CoreAllowedOriginStore>();
        AddCoreODataExposures(services, routeDefinitions);

        return services;
    }

    private static void AddCoreAspNetExposures(IServiceCollection services)
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

    private static void AddCoreODataExposures(
        IServiceCollection services,
        IEnumerable<CoreApiRouteDefinition> routeDefinitions)
    {
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
