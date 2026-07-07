using cCoder.Core.Exposures;
using cCoder.Core.Models;
using cCoder.Data.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace cCoder.Core;

public partial class CoreApiBuilderOptions
{
    private IEnumerable<CoreApiRouteDefinition> BuildRouteDefinitions() =>
        routeContributors
            .Select(route => new CoreApiRouteDefinition(
                Name: GetContextName(route.Key),
                RoutePath: route.Key,
                RouteModel: BuildRouteModel(route.Value)))
            .OrderBy(route => route.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private void RegisterContext(
        string routePath,
        Action<ODataConventionModelBuilder> configureModel)
    {
        string normalizedRoutePath = EnsureRoutePath(routePath, "Core");

        if (!routeContributors.TryGetValue(normalizedRoutePath, out List<Action<ODataConventionModelBuilder>> contributors))
        {
            contributors = [];
            routeContributors[normalizedRoutePath] = contributors;
        }

        contributors.Add(configureModel);
    }

    private void RegisterDomainContext(
        string routePath,
        bool includeLegacyCoreContext,
        Action<ODataConventionModelBuilder> configureModel)
    {
        if (coreConfiguration?.AggregateDomains == true)
        {
            RegisterContext("Api/Core", configureModel);
            return;
        }

        RegisterContext(routePath, configureModel);
    }

    private void RegisterApiInfos(IEnumerable<CoreApiRouteDefinition> routes)
    {
        services.AddSingleton(new ApiInfo
        {
            Kind = "Context",
            Name = "Core",
            Url = "Core",
            SwaggerDef = "/swagger/Core/swagger.json",
        });

        foreach (CoreApiRouteDefinition route in routes)
        {
            if (string.Equals(route.Name, "Core", StringComparison.OrdinalIgnoreCase))
                continue;

            services.AddSingleton(new ApiInfo
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
            contributor(builder);

        return builder.GetEdmModel();
    }

    private static string GetContextName(string routePath)
    {
        string normalizedRoutePath = EnsureRoutePath(routePath, "Core");
        int lastSlashIndex = normalizedRoutePath.LastIndexOf('/');
        return lastSlashIndex < 0
            ? normalizedRoutePath
            : normalizedRoutePath[(lastSlashIndex + 1)..];
    }

    private static string EnsureRoutePath(string routePath, string defaultContext)
    {
        if (string.IsNullOrWhiteSpace(routePath))
            return $"Api/{defaultContext}";

        return routePath.Trim().Trim('/');
    }

    private void ConfigureDomainRouting<TDomainConfiguration>(
        TDomainConfiguration configuration,
        string domainName,
        CoreDomainsConfig defaults)
    {
        Type configType = typeof(TDomainConfiguration);
        string rootPath = defaults.RootPath.Trim().TrimEnd('/');
        string routePath = coreConfiguration?.AggregateDomains == true
            ? $"{rootPath}/Core"
            : $"{rootPath}/{domainName}";

        configType.GetProperty("RootPath")?.SetValue(configuration, routePath);
        configType.GetProperty("IncludeLegacyCoreContext")?.SetValue(configuration, false);
    }

    private void ApplyDomainRouteMode<TDomainConfiguration>(
        TDomainConfiguration configuration,
        string domainName)
    {
        Type configType = typeof(TDomainConfiguration);

        configType.GetProperty("RootPath")?.SetValue(configuration, $"Api/{domainName}");
        configType.GetProperty("IncludeLegacyCoreContext")?.SetValue(configuration, false);
    }

    private static CoreApiRouteDefinition[] EnsureRequiredRoutes(
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        CoreApiRouteDefinition[] definitions = (routes ?? [])
            .Where(route => route is not null)
            .ToArray();

        if (definitions.Any(route => string.Equals(route.Name, "Security", StringComparison.OrdinalIgnoreCase)))
            return definitions;

        return
        [
            .. definitions,
            new CoreApiRouteDefinition(
                "Security",
                "Api/Security",
                BuildRouteModel([static builder => builder.ConfigureCoreSecurityApiModel()]))
        ];
    }

}
