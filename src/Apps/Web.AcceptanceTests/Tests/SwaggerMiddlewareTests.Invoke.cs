// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using FluentAssertions;
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