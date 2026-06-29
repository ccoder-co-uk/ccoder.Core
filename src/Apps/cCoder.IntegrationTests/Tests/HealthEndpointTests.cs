using FluentAssertions;
using cCoder.IntegrationTests.Infrastructure;
using Xunit;

namespace cCoder.IntegrationTests.Tests;

[Collection(IntegrationAcceptanceCollection.Name)]
public sealed class HealthEndpointTests(IntegrationAcceptanceFixture fixture)
{
    [Fact]
    public async Task ShouldReturnOkFromAllApps()
    {
        string web = await fixture.WebClient.GetStringAsync("Health");
        string hostedServices = await fixture.HostedServicesClient.GetStringAsync("Health");
        string workflow = await GetWorkflowHealthAsync();

        web.Should().Be("OK");
        hostedServices.Should().Be("OK");
        workflow.Should().Be("OK");
    }

    private async Task<string> GetWorkflowHealthAsync()
    {
        using HttpClient client = new()
        {
            BaseAddress = fixture.WorkflowBaseAddress
        };

        return await client.GetStringAsync("Health");
    }
}
