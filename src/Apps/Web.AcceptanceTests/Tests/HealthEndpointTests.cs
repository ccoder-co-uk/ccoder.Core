// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;
using Web.AcceptanceTests.Infrastructure;
using Xunit;

namespace Web.AcceptanceTests.Tests;

[Collection(WebAcceptanceCollection.Name)]
public sealed class HealthEndpointTests(WebAcceptanceFixture fixture)
{
    [Fact]
    public async Task ShouldReturnOk()
    {
        string content = await fixture.Client.GetStringAsync(requestUri: "Health");

        content.Should()
            .Be(expected: "OK");
    }
}