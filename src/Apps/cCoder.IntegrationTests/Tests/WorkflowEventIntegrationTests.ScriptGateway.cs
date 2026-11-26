// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace cCoder.IntegrationTests.Tests;

public sealed partial class WorkflowEventIntegrationTests
{
    [Fact]
    public async Task ScriptGateway_ReturnsResultFromAuthenticatedStandaloneWorker()
    {
        // Given
        string token = await fixture.CreateAuthTokenAsync(userId: AdminUserId);

        using HttpRequestMessage request = new(method: HttpMethod.Post, requestUri: "/Api/ExecuteScript")
        {
            Content = new StringContent(content: "return 42;", encoding: Encoding.UTF8, mediaType: "text/plain")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue(scheme: "Bearer", parameter: token);

        // When
        using HttpResponseMessage response = await fixture.WebClient.SendAsync(request: request);
        string result = await response.Content.ReadAsStringAsync();

        // Then
        string diagnostics = string.Join(
            separator: "\n",
            values: (fixture.WorkflowOutput + "\n" + fixture.WebOutput)
                .Split(separator: '\n')
                .Where(predicate: line => line.Contains(value: "useDetails")
                    || line.Contains(value: "HttpRequestException")
                    || line.Contains(value: "Request finished")));

        response.StatusCode
            .Should()
            .Be(expected: HttpStatusCode.OK, because: "{0}\n{1}", becauseArgs: [result, diagnostics]);

        result
            .Should()
            .Be(expected: "42");
    }
}