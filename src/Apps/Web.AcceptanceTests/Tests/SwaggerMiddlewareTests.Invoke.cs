// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Web.AcceptanceTests.Infrastructure;
using Xunit;


namespace Web.AcceptanceTests.Tests.Api;

public sealed partial class SwaggerMiddlewareTests
{
    [Theory]
    [InlineData("/swagger/Core/swagger.json")]
    [InlineData("/swagger/AppSecurity/swagger.json")]
    [InlineData("/swagger/ContentManagement/swagger.json")]
    [InlineData("/swagger/DocumentManagement/swagger.json")]
    [InlineData("/swagger/Logging/swagger.json")]
    [InlineData("/swagger/Mail/swagger.json")]
    [InlineData("/swagger/Packaging/swagger.json")]
    [InlineData("/swagger/Security/swagger.json")]
    [InlineData("/swagger/Workflow/swagger.json")]
    [InlineData("/swagger/index.html")]
    public async Task Invoke_ReturnsSwaggerDefinition(string baseUrl)
    {
        // Given
        int actualStatusCode;

        // When
        actualStatusCode = await InvokeAsync(baseUrl: baseUrl);

        // Then
        actualStatusCode.Should()
            .Be(expected: (int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task Invoke_ReturnsForbiddenWithoutMetadataPrivilege()
    {
        // Given
        using HttpClient client = CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                scheme: "Bearer",
                parameter: AcceptanceApplicationSeeder
                    .MetadataOrdinaryUserToken);

        // When
        using HttpResponseMessage response =
            await client.GetAsync(
                requestUri: "/swagger/Core/swagger.json");

        // Then
        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalid-token")]
    public async Task Invoke_ReturnsUnauthorizedWithoutValidAuthentication(
        string token)
    {
        // Given
        using HttpClient client = CreateClient();

        if (token is not null)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    scheme: "Bearer",
                    parameter: token);
        }

        // When
        using HttpResponseMessage response =
            await client.GetAsync(
                requestUri: "/swagger/Core/swagger.json");

        // Then
        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.Unauthorized);

        response.Headers.WwwAuthenticate
            .Should()
            .ContainSingle(predicate: value =>
                value.Scheme == "Bearer");
    }

    [Fact]
    public async Task Invoke_ReturnsNotFoundForLegacyV1SwaggerDefinition()
    {
        // Given
        const string baseUrl = "/swagger/v1/swagger.json";

        // When
        using HttpResponseMessage response = await Client.GetAsync(requestUri: baseUrl);

        // Then
        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.NotFound);
    }
}