using cCoder.Core.Exposures.Cors;
using cCoder.Core.Exposures.Formatters;
using cCoder.Core.Exposures;
using cCoder.Core.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

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
        AddCoreODataRouteMode(services);

        return services;
    }

    private static void AddCoreAspNetExposures(IServiceCollection services)
    {
        CoreConfiguration coreConfiguration = GetRegisteredCoreConfiguration(services);

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

            if (coreConfiguration?.AggregateDomains != true)
                options.Conventions.Add(new SplitDomainApplicationModelConvention());
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

    private static void AddCoreODataRouteMode(IServiceCollection services)
    {
        CoreConfiguration coreConfiguration = GetRegisteredCoreConfiguration(services);

        services.PostConfigure<ODataOptions>(options =>
        {
            if (coreConfiguration?.AggregateDomains != true)
                return;

            string[] aggregateDomainRoutes =
            [
                "Api/AppSecurity",
                "Api/ContentManagement",
                "Api/DocumentManagement",
                "Api/Logging",
                "Api/Mail",
                "Api/Workflow",
            ];

            foreach (string route in aggregateDomainRoutes)
                options.RouteComponents.Remove(route);
        });
    }

    private static CoreConfiguration GetRegisteredCoreConfiguration(IServiceCollection services) =>
        services
            .Where(descriptor => descriptor.ServiceType == typeof(CoreConfiguration))
            .Select(descriptor => descriptor.ImplementationInstance)
            .OfType<CoreConfiguration>()
            .LastOrDefault();

    private static HttpContext CreateHttpContext(HttpContext httpContext)
    {
        if (httpContext is not null)
            return httpContext;

        DefaultHttpContext fallbackContext = new();
        fallbackContext.Features.Set<ISessionFeature>(new NoOpSessionFeature());
        return fallbackContext;
    }

    private sealed class SplitDomainApplicationModelConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            if (!string.Equals(action.Controller.ControllerName, "App", StringComparison.Ordinal))
                return;

            for (int index = action.Selectors.Count - 1; index >= 0; index--)
            {
                string template = action.Selectors[index].AttributeRouteModel?.Template;

                if (template?.StartsWith("Api/Core/App", StringComparison.OrdinalIgnoreCase) == true)
                    action.Selectors.RemoveAt(index);
            }
        }
    }
}
