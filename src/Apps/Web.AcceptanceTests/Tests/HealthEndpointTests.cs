// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;
using Web.AcceptanceTests.Infrastructure;
using Xunit;

namespace Web.AcceptanceTests.Tests;

[Collection(WebAcceptanceCollection.Name)]
public sealed partial class HealthEndpointTests(WebAcceptanceFixture fixture)
{
    [Fact]
    public async Task ShouldReturnOk()
    {
        // Given
        const string healthEndpoint = "Health";

        // When
        string content = await fixture.Client.GetStringAsync(requestUri: healthEndpoint);

        // Then
        content.Should()
            .Be(expected: "OK");
    }
}