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
        string content = await fixture.Client.GetStringAsync("Health");

        content.Should().Be("OK");
    }
}
