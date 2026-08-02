// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using FluentAssertions;
using Web.AcceptanceTests.Infrastructure;
using Xunit;


namespace Web.AcceptanceTests.Tests.Api;

[Collection(WebAcceptanceCollection.Name)]
public sealed partial class SwaggerMiddlewareTests(WebAcceptanceFixture fixture)
{
    private HttpClient Client { get; } =
        fixture.CreateMetadataClient();

    private WebAcceptanceFixture Fixture { get; } = fixture;

    private HttpClient CreateClient() =>
        Fixture.Factory.CreateClient();

    private async Task<int> InvokeAsync(string baseUrl)
    {
        using HttpResponseMessage response = await Client.GetAsync(requestUri: baseUrl);
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: content);

        return (int)response.StatusCode;
    }
}