// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;
using HostedServices.AcceptanceTests.Infrastructure;
using Xunit;

namespace HostedServices.AcceptanceTests.Tests.Api;

[Collection(HostedServicesAcceptanceCollection.Name)]
public sealed partial class HealthEndpointTests(HostedServicesAcceptanceFixture fixture)
{
    [Fact]
    public async Task ShouldReturnOk()
    {
        // Given
        const string requestUri = "Health";

        // When
        string content = await fixture.Client.GetStringAsync(requestUri: requestUri);

        // Then
        content.Should()
            .Be(expected: "OK");
    }
}