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
        string content = await fixture.Client.GetStringAsync("Health");

        content.Should().Be("OK");
    }
}
