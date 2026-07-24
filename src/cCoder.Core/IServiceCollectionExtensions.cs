// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.Eventing;
using cCoder.Eventing.Models;
using cCoder.Security.Objects.Events;
using Microsoft.OpenApi;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    public static void AddCoreWeb(
        this IServiceCollection services,
        Action<CoreApiBuilderOptions> configure = null)
    {
        ConfigureDefaultLogging(services: services, configuration: GetRequiredConfiguration(services: services));
        AddCoreApi(services: services, setupAction: configure ?? (_ => { }));
        AddCoreFirstTimeSetup(services: services);
    }

    public static void AddCoreHostedServices(
        this IServiceCollection services,
        Action<CoreBuilderOptions> configure = null)
    {
        ConfigureDefaultLogging(services: services, configuration: GetRequiredConfiguration(services: services));
        AddCore(services: services, setupAction: configure ?? (_ => { }));
        AddCoreAspNetExposures(services: services);
    }

    internal static IServiceCollection AddCoreApi(
        this IServiceCollection services,
        IEnumerable<CoreApiRouteDefinition> routeDefinitions = null) =>
        AddCoreExposures(services: services, routeDefinitions: routeDefinitions);

    internal static IServiceCollection AddCoreApiDocumentation(
        this IServiceCollection services,
        params string[] apiContexts)
    {
        CoreApiRouteDefinition[] routes = GetRouteDefinitions(apiContexts: apiContexts);
        return services.AddCoreApiDocumentation(routes: routes);
    }

    internal static IServiceCollection AddCoreApiDocumentation(
        this IServiceCollection services,
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        CoreApiRouteDefinition[] definitions = GetRouteDefinitions(routes: routes);

        services.AddSwaggerGen(setupAction: c =>
        {
            c.ResolveConflictingActions(resolver: apiDescriptions => apiDescriptions.First());
            c.CustomSchemaIds(schemaIdSelector: type => type.FullName?.Replace(oldChar: '+', newChar: '.') ?? type.Name);
            AddSwaggerDocuments(options: c, routes: definitions);

            c.DocInclusionPredicate(
predicate: (documentName, apiDescription) =>
                    ShouldIncludeInDocument(documentName: documentName, relativePath: apiDescription.RelativePath, routes: definitions));

            c.AddSecurityDefinition(name: "bearer", securityScheme: new OpenApiSecurityScheme
            {
                Description = @"Authorization header using the Bearer scheme. \r\n\r\n 
                        Enter 'Bearer' [space] and then your token in the text input below.
                        \r\n\r\nExample: 'bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "bearer",
            });
        });

        return services;
    }

    internal static void AddCoreEventing(
        this IServiceCollection services,
        IEnumerable<EventProvider> eventProviders)
    {
        services.AddEventing(configure: configuration =>
        {
            configuration.EventProviders =
                (eventProviders ?? []).Where(predicate: provider => provider is not null)
                .ToArray();
        });

        services.AddEventingForType<SecurityAccountEvent>();
    }

    private static void AddCoreApi(
        IServiceCollection services,
        Action<CoreApiBuilderOptions> setupAction)
    {
        CoreApiBuilderOptions config = new(services);
        setupAction(obj: config);
        config.Apply();
    }

    private static void AddCore(
        IServiceCollection services,
        Action<CoreBuilderOptions> setupAction)
    {
        CoreBuilderOptions config = new(services);
        setupAction(obj: config);
        config.Apply();
    }

    private static IConfiguration GetRequiredConfiguration(IServiceCollection services)
    {
        IConfiguration configuration = services
            .Where(predicate: descriptor => typeof(IConfiguration).IsAssignableFrom(c: descriptor.ServiceType))
            .Select(selector: descriptor => descriptor.ImplementationInstance)
            .OfType<IConfiguration>()
            .LastOrDefault();

        if (configuration is not null)
        {
            return configuration;
        }

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetService<IConfiguration>()
            ?? throw new InvalidOperationException(
                "IConfiguration must already be registered on the IServiceCollection before calling AddCoreWeb or AddCoreHostedServices.");
    }

    private static void ConfigureDefaultLogging(
        IServiceCollection services,
        IConfiguration configuration) =>
        services.AddLogging(configure: logBuilder =>
        {
            logBuilder.ClearProviders();
            logBuilder.AddFilter(levelFilter: level => level >= LogLevel.Debug);

            logBuilder.AddSimpleConsole(configure: options =>
            {
                options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss ";
                options.SingleLine = true;
            });

            logBuilder.AddConfiguration(configuration: configuration.GetSection(key: "Logging"));
        });

    private static CoreApiRouteDefinition[] GetRouteDefinitions(IEnumerable<string> apiContexts) =>
        GetRouteDefinitions(routes: (apiContexts ?? [])
            .Where(predicate: context => !string.IsNullOrWhiteSpace(value: context))
            .Select(selector: context => new CoreApiRouteDefinition(
                context,
                $"Api/{context}",
                null)));

    private static CoreApiRouteDefinition[] GetRouteDefinitions(
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        CoreApiRouteDefinition coreRoute = new("Core", "Api/Core", null);

        return [coreRoute, .. (routes ?? [])
            .Where(predicate: route => route is not null && !string.IsNullOrWhiteSpace(value: route.Name))
            .GroupBy(keySelector: route => route.Name,comparer: StringComparer.OrdinalIgnoreCase)
            .Select(selector: group => group.First())
            .Where(predicate: route => !string.Equals(a: route.Name,b: "Core",comparisonType: StringComparison.OrdinalIgnoreCase))];
    }

    private static void AddSwaggerDocuments(
        Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options,
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        foreach (CoreApiRouteDefinition route in routes)
        {
            options.SwaggerDoc(name: route.Name, info: new OpenApiInfo
            {
                Title = $"{route.Name} API definition",
                Version = route.Name,
            });
        }

        options.SwaggerDoc(name: "v1", info: new OpenApiInfo
        {
            Title = "Corporate LinX V7 API definition",
            Version = "v1",
        });
    }

    private static bool ShouldIncludeInDocument(
        string documentName,
        string relativePath,
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        if (string.IsNullOrWhiteSpace(value: relativePath))
        {
            return false;
        }

        if (string.Equals(a: documentName, b: "v1", comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            documentName = "Core";
        }

        string path = NormalizePath(relativePath: relativePath);

        if (string.Equals(a: documentName, b: "Core", comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            return IsCoreRoute(path: path, routes: routes);
        }

        CoreApiRouteDefinition route = routes.FirstOrDefault(predicate: candidate =>
            string.Equals(a: candidate.Name, b: documentName, comparisonType: StringComparison.OrdinalIgnoreCase));

        return route is not null && MatchesRoutePath(path: path, routePath: route.RoutePath);
    }

    private static bool IsCoreRoute(string path, IEnumerable<CoreApiRouteDefinition> routes)
    {
        if (MatchesContextRoute(path: path, context: "Core"))
        {
            return true;
        }

        if (!path.Equals(value: "/Api", comparisonType: StringComparison.OrdinalIgnoreCase)
            && !path.StartsWith(value: "/Api/", comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        foreach (CoreApiRouteDefinition route in routes.Where(predicate: route =>
                     !string.Equals(a: route.Name, b: "Core", comparisonType: StringComparison.OrdinalIgnoreCase)
                     && !string.Equals(a: route.Name, b: "v1", comparisonType: StringComparison.OrdinalIgnoreCase)))
        {
            if (MatchesRoutePath(path: path, routePath: route.RoutePath))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesRoutePath(string path, string routePath)
    {
        string prefix = NormalizePath(relativePath: routePath);

        return path.Equals(value: prefix, comparisonType: StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(value: $"{prefix}/", comparisonType: StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesContextRoute(string path, string context)
    {
        string prefix = $"/Api/{context}";

        return path.Equals(value: prefix, comparisonType: StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(value: $"{prefix}/", comparisonType: StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string relativePath) =>
        relativePath.StartsWith(value: '/') ? relativePath : $"/{relativePath}";
}