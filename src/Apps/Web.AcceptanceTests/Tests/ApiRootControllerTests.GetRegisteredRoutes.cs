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
            Path.Combine(AppContext.BaseDirectory, "ActualEndpointManifest.txt"),
            actual
        );

        // Then
        actual.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
    }
}



