// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.Data.Models;
using FluentAssertions;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Xunit;

namespace cCoder.Core.Tests.Api;

public sealed partial class CoreODataContextRegistrationTests
{
    [Fact]
    public void ShouldNotAdvertiseCoreAsAnODataContext()
    {
        // Given
        ServiceCollection services = new();

        // When
        services.AddCoreApiContexts(contextNames: []);

        // Then
        services.BuildServiceProvider(
                options: new ServiceProviderOptions())
            .GetServices<ApiInfo>()
            .Should()
            .NotContain(predicate: info =>
                string.Equals(
                    a: info.Name,
                    b: "Core",
                    comparisonType: StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ShouldNotRegisterCoreAsAnODataRouteComponent()
    {
        // Given
        ServiceCollection services = new();
        EdmModel model = new();
        services.AddLogging();

        CoreApiRouteDefinition[] routeDefinitions =
        [
            new(Name: "Core", RoutePath: "Api/Core", RouteModel: model),
            new(Name: "Security", RoutePath: "Api/Security", RouteModel: model),
        ];

        // When
        services.AddCoreODataExposures(
            routeDefinitions: routeDefinitions);

        // Then
        ODataOptions options = services.BuildServiceProvider(
                options: new ServiceProviderOptions())
            .GetRequiredService<IOptions<ODataOptions>>()
            .Value;

        options.RouteComponents
            .Should()
            .ContainKey(expected: "Api/Security");

        options.RouteComponents
            .Should()
            .NotContainKey(unexpected: "Api/Core");
    }
}