// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.Core.Exposures;
using cCoder.Data.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace cCoder.Core;

public partial class CoreApiBuilderOptions
{
    private IEnumerable<CoreApiRouteDefinition> BuildRouteDefinitions() =>
        routeContributors
            .Select(selector: route => new CoreApiRouteDefinition(
                Name: GetContextName(routePath: route.Key),
                RoutePath: route.Key,
                RouteModel: BuildRouteModel(contributors: route.Value)))
            .OrderBy(keySelector: route => route.Name, comparer: StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private void RegisterContext(
        string routePath,
        Action<ODataConventionModelBuilder> configureModel)
    {
        string normalizedRoutePath = EnsureRoutePath(routePath: routePath, defaultContext: "Core");

        if (!routeContributors.TryGetValue(key: normalizedRoutePath, value: out List<Action<ODataConventionModelBuilder>> contributors))
        {
            contributors = [];
            routeContributors[normalizedRoutePath] = contributors;
        }

        contributors.Add(item: configureModel);
    }

    private void RegisterDomainContext(
        string routePath,
        bool includeLegacyCoreContext,
        Action<ODataConventionModelBuilder> configureModel)
    {
        if (coreConfiguration?.AggregateDomains == true)
        {
            RegisterContext(routePath: "Api/Core", configureModel: configureModel);
            return;
        }

        RegisterContext(routePath: routePath, configureModel: configureModel);
    }

    private void RegisterApiInfos(IEnumerable<CoreApiRouteDefinition> routes)
    {
        services.AddSingleton(implementationInstance: new ApiInfo
        {
            Kind = "Context",
            Name = "Core",
            Url = "Core",
            SwaggerDef = "/swagger/Core/swagger.json",
        });

        foreach (CoreApiRouteDefinition route in routes)
        {
            if (string.Equals(a: route.Name, b: "Core", comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            services.AddSingleton(implementationInstance: new ApiInfo
            {
                Kind = "Context",
                Name = route.Name,
                Url = route.Name,
                SwaggerDef = $"/swagger/{route.Name}/swagger.json",
            });
        }
    }

    private static IEdmModel BuildRouteModel(
        IEnumerable<Action<ODataConventionModelBuilder>> contributors)
    {
        ODataConventionModelBuilder builder = new();

        foreach (Action<ODataConventionModelBuilder> contributor in contributors)
        {
            contributor(obj: builder);
        }

        return builder.GetEdmModel();
    }

    private static string GetContextName(string routePath)
    {
        string normalizedRoutePath = EnsureRoutePath(routePath: routePath, defaultContext: "Core");
        int lastSlashIndex = normalizedRoutePath.LastIndexOf(value: '/');

        return lastSlashIndex < 0
            ? normalizedRoutePath
            : normalizedRoutePath[(lastSlashIndex + 1)..];
    }

    private static string EnsureRoutePath(string routePath, string defaultContext)
    {
        if (string.IsNullOrWhiteSpace(value: routePath))
        {
            return $"Api/{defaultContext}";
        }

        return routePath.Trim()
            .Trim(trimChar: '/');
    }

    private void ConfigureDomainRouting<TDomainConfiguration>(
        TDomainConfiguration configuration,
        string domainName,
        CoreDomainsConfig defaults)
    {
        Type configType = typeof(TDomainConfiguration);

        string rootPath = defaults.RootPath.Trim()
            .TrimEnd(trimChar: '/');

        string routePath = coreConfiguration?.AggregateDomains == true
            ? $"{rootPath}/Core"
            : $"{rootPath}/{domainName}";

        configType.GetProperty(name: "RootPath")?.SetValue(obj: configuration, value: routePath);
        configType.GetProperty(name: "IncludeLegacyCoreContext")?.SetValue(obj: configuration, value: false);
    }

    private void ApplyDomainRouteMode<TDomainConfiguration>(
        TDomainConfiguration configuration,
        string domainName)
    {
        Type configType = typeof(TDomainConfiguration);

        configType.GetProperty(name: "RootPath")?.SetValue(obj: configuration, value: $"Api/{domainName}");
        configType.GetProperty(name: "IncludeLegacyCoreContext")?.SetValue(obj: configuration, value: false);
    }

    private static CoreApiRouteDefinition[] EnsureRequiredRoutes(
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        CoreApiRouteDefinition[] definitions = (routes ?? [])
            .Where(predicate: route => route is not null)
            .ToArray();

        if (definitions.Any(predicate: route => string.Equals(a: route.Name, b: "Security", comparisonType: StringComparison.OrdinalIgnoreCase)))
        {
            return definitions;
        }

        return
        [
            .. definitions,
            new CoreApiRouteDefinition(
                "Security",
                "Api/Security",
                BuildRouteModel(contributors: [static builder => builder.ConfigureCoreSecurityApiModel()]))
        ];
    }

}