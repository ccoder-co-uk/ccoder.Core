// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;
using Web.AcceptanceTests.Infrastructure;
using Xunit;


namespace Web.AcceptanceTests.Tests.Api;

public sealed partial class ApiRootControllerTests
{
    [Fact]
    public void ShouldMatchEndpointManifestForGetRegisteredRoutes()
    {
        // Given
        string[] expected = EndpointManifestReader.LoadExpectedRoutes();

        // When
        string[] actual = GetRegisteredRoutes();

        File.WriteAllLines(
path:             Path.Combine(path1: AppContext.BaseDirectory,path2: "ActualEndpointManifest.txt"),contents:             actual
        );

        // Then
        actual.Should()
            .BeEquivalentTo(expectation: expected,config: options => options.WithStrictOrdering());
    }
}