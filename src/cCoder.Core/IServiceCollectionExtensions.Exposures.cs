// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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
        AddCoreAspNetExposures(services: services);
        AddCoreBrokers(services: services);
        AddCoreFoundationServices(services: services);
        AddCoreProcessingServices(services: services);
        AddCoreOrchestrationServices(services: services);
        services.AddScoped<ICoreAllowedOriginStore, CoreAllowedOriginStore>();
        AddCoreODataExposures(services: services, routeDefinitions: routeDefinitions);
        AddCoreODataRouteMode(services: services);

        return services;
    }

    private static void AddCoreAspNetExposures(IServiceCollection services)
    {
        CoreConfiguration coreConfiguration = GetRegisteredCoreConfiguration(services: services);

        services.AddRouting();
        services.AddResponseCompression();

        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddScoped(serviceType: typeof(HttpContext), implementationFactory: ctx => CreateHttpContext(httpContext: ctx.GetService<IHttpContextAccessor>()?.HttpContext));
        services.AddScoped(serviceType: typeof(HttpRequest), implementationFactory: ctx => ctx.GetRequiredService<HttpContext>().Request);
        services.AddScoped(serviceType: typeof(ISession), implementationFactory: ctx =>
        {
            HttpContext httpContext = ctx.GetRequiredService<HttpContext>();
            return httpContext.Features.Get<ISessionFeature>()?.Session ?? NoOpSession.Instance;
        });

        services.AddSession();
        services.AddHsts(configureOptions: options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromMinutes(minutes: 60);
        });

        services.AddMvc(setupAction: options =>
        {
            options.EnableEndpointRouting = false;
            options.OutputFormatters.Add(item: new XmlFormatter());
            options.OutputFormatters.Add(item: new CsvFormatter());
            options.OutputFormatters.Add(item: new ExcelFormatter());

            if (coreConfiguration?.AggregateDomains != true)
            {
                options.Conventions.Add(actionModelConvention: new SplitDomainApplicationModelConvention());
            }
        });
        services.AddRazorPages();

        services.Configure<KestrelServerOptions>(configureOptions: options =>
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
            .Where(predicate: route =>
                route is not null
                && (string.Equals(a: route.Name, b: "Core", comparisonType: StringComparison.OrdinalIgnoreCase)
                    || string.Equals(a: route.RoutePath, b: "Api/Core", comparisonType: StringComparison.OrdinalIgnoreCase)
                    || string.Equals(a: route.Name, b: "Security", comparisonType: StringComparison.OrdinalIgnoreCase)
                    || string.Equals(a: route.RoutePath, b: "Api/Security", comparisonType: StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        services.AddControllers().AddOData(setupAction: opt =>
        {
            opt.RouteOptions.EnableQualifiedOperationCall = false;
            opt.EnableAttributeRouting = true;
            opt.RouteOptions.EnableKeyAsSegment = false;
            opt.Expand()
                .Count()
                .Filter()
                .Select()
                .OrderBy()
                .SetMaxTop(maxTopValue: 1000);

            foreach (CoreApiRouteDefinition routeDefinition in definitions)
            {
                _ = opt.AddRouteComponents(
routePrefix: routeDefinition.RoutePath, model: routeDefinition.RouteModel, batchHandler: batchHandler);
            }
        });
    }

    private static void AddCoreODataRouteMode(IServiceCollection services)
    {
        CoreConfiguration coreConfiguration = GetRegisteredCoreConfiguration(services: services);

        services.PostConfigure<ODataOptions>(configureOptions: options =>
        {
            if (coreConfiguration?.AggregateDomains != true)
            {
                return;
            }

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
            {
                options.RouteComponents.Remove(key: route);
            }
        });
    }

    private static CoreConfiguration GetRegisteredCoreConfiguration(IServiceCollection services) =>
        services
            .Where(predicate: descriptor => descriptor.ServiceType == typeof(CoreConfiguration))
            .Select(selector: descriptor => descriptor.ImplementationInstance)
            .OfType<CoreConfiguration>()
            .LastOrDefault();

    private static HttpContext CreateHttpContext(HttpContext httpContext)
    {
        if (httpContext is not null)
        {
            return httpContext;
        }

        DefaultHttpContext fallbackContext = new();
        fallbackContext.Features.Set<ISessionFeature>(instance: new NoOpSessionFeature());
        return fallbackContext;
    }

    private sealed class SplitDomainApplicationModelConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            if (!string.Equals(a: action.Controller.ControllerName, b: "App", comparisonType: StringComparison.Ordinal))
            {
                return;
            }

            for (int index = action.Selectors.Count - 1; index >= 0; index--)
            {
                string template = action.Selectors[index].AttributeRouteModel?.Template;

                if (template?.StartsWith(value: "Api/Core/App", comparisonType: StringComparison.OrdinalIgnoreCase) == true)
                {
                    action.Selectors.RemoveAt(index: index);
                }
            }
        }
    }
}