using cCoder.Core.Models;
using Microsoft.OpenApi;


namespace cCoder.Core;

internal static class CoreApiDocumentationServiceCollectionExtensions
{
    internal static IServiceCollection AddCoreApiDocumentation(
        this IServiceCollection services,
        params string[] apiContexts)
    {
        CoreApiRouteDefinition[] routes = GetRouteDefinitions(apiContexts);
        return services.AddCoreApiDocumentation(routes);
    }

    internal static IServiceCollection AddCoreApiDocumentation(
        this IServiceCollection services,
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        CoreApiRouteDefinition[] definitions = GetRouteDefinitions(routes);

        services.AddSwaggerGen(c =>
        {
            c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            c.CustomSchemaIds(type => type.FullName?.Replace('+', '.') ?? type.Name);
            AddSwaggerDocuments(c, definitions);
            c.DocInclusionPredicate(
                (documentName, apiDescription) =>
                    ShouldIncludeInDocument(documentName, apiDescription.RelativePath, definitions));

            c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
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

    private static CoreApiRouteDefinition[] GetRouteDefinitions(IEnumerable<string> apiContexts) =>
        GetRouteDefinitions((apiContexts ?? [])
            .Where(context => !string.IsNullOrWhiteSpace(context))
            .Select(context => new CoreApiRouteDefinition(
                context,
                $"Api/{context}",
                null)));

    private static CoreApiRouteDefinition[] GetRouteDefinitions(
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        CoreApiRouteDefinition coreRoute = new("Core", "Api/Core", null);

        return [coreRoute, .. (routes ?? [])
            .Where(route => route is not null && !string.IsNullOrWhiteSpace(route.Name))
            .GroupBy(route => route.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Where(route => !string.Equals(route.Name, "Core", StringComparison.OrdinalIgnoreCase))];
    }

    private static void AddSwaggerDocuments(
        Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options,
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        foreach (CoreApiRouteDefinition route in routes)
        {
            options.SwaggerDoc(route.Name, new OpenApiInfo
            {
                Title = $"{route.Name} API definition",
                Version = route.Name,
            });
        }

        options.SwaggerDoc("v1", new OpenApiInfo
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
        if (string.IsNullOrWhiteSpace(relativePath))
            return false;

        if (string.Equals(documentName, "v1", StringComparison.OrdinalIgnoreCase))
            documentName = "Core";

        string path = NormalizePath(relativePath);

        if (string.Equals(documentName, "Core", StringComparison.OrdinalIgnoreCase))
            return IsCoreRoute(path, routes);

        CoreApiRouteDefinition route = routes.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, documentName, StringComparison.OrdinalIgnoreCase));

        return route is not null && MatchesRoutePath(path, route.RoutePath);
    }

    private static bool IsCoreRoute(string path, IEnumerable<CoreApiRouteDefinition> routes)
    {
        if (MatchesContextRoute(path, "Core"))
            return true;

        if (!path.Equals("/Api", StringComparison.OrdinalIgnoreCase)
            && !path.StartsWith("/Api/", StringComparison.OrdinalIgnoreCase))
            return false;

        foreach (CoreApiRouteDefinition route in routes.Where(route =>
                     !string.Equals(route.Name, "Core", StringComparison.OrdinalIgnoreCase)
                     && !string.Equals(route.Name, "v1", StringComparison.OrdinalIgnoreCase)))
        {
            if (MatchesRoutePath(path, route.RoutePath))
                return false;
        }

        return true;
    }

    private static bool MatchesRoutePath(string path, string routePath)
    {
        string prefix = NormalizePath(routePath);
        return path.Equals(prefix, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith($"{prefix}/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesContextRoute(string path, string context)
    {
        string prefix = $"/Api/{context}";
        return path.Equals(prefix, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith($"{prefix}/", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string relativePath) =>
        relativePath.StartsWith('/') ? relativePath : $"/{relativePath}";
}



