// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;
using cCoder.IntegrationTests.Infrastructure;
using Xunit;

namespace cCoder.IntegrationTests.Tests;

[Collection(IntegrationAcceptanceCollection.Name)]
public sealed partial class HealthEndpointTests(IntegrationAcceptanceFixture fixture)
{
    [Fact]
    public async Task ShouldReturnOkFromAllApps()
    {
        // Given
        const string healthEndpoint = "Health";

        // When
        string web = await fixture.WebClient.GetStringAsync(requestUri: healthEndpoint);
        string hostedServices = await fixture.HostedServicesClient.GetStringAsync(requestUri: healthEndpoint);
        string workflow = await GetWorkflowHealthAsync();

        // Then
        web.Should()
            .Be(expected: "OK");

        hostedServices.Should()
            .Be(expected: "OK");

        workflow.Should()
            .Be(expected: "OK");
    }

    private async Task<string> GetWorkflowHealthAsync()
    {
        using HttpClient client = new()
        {
            BaseAddress = fixture.WorkflowBaseAddress
        };

        return await client.GetStringAsync(requestUri: "Health");
    }
}