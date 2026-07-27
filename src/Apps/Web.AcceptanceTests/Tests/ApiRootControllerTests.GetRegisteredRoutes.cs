// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;
using System.Text.Json;
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
        string[] missing = expected
            .Except(second: actual, comparer: StringComparer.Ordinal)
            .ToArray();

        string[] unexpected = actual
            .Except(second: expected, comparer: StringComparer.Ordinal)
            .ToArray();

        actual.Should()
            .BeEquivalentTo(
                expectation: expected,
                because: $"missing routes: {JsonSerializer.Serialize(value: missing)}; "
                    + $"unexpected routes: {JsonSerializer.Serialize(value: unexpected)}");
    }
}