// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;
using HostedServices.AcceptanceTests.Infrastructure;
using Xunit;

namespace HostedServices.AcceptanceTests.Tests.Api;

[Collection(HostedServicesAcceptanceCollection.Name)]
public sealed class HealthEndpointTests(HostedServicesAcceptanceFixture fixture)
{
    [Fact]
    public async Task ShouldReturnOk()
    {
        string content = await fixture.Client.GetStringAsync(requestUri: "Health");

        content.Should()
            .Be(expected: "OK");
    }
}