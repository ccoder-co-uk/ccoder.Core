// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using FluentAssertions;
using Xunit;


namespace Web.AcceptanceTests.Tests.Api;

public sealed partial class CoreODataMiddlewareTests
{
    [Fact]
    public async Task Invoke_ReturnsServiceDocument()
    {
        // Given
        string requestUri = BaseUrl;

        // When
        using HttpResponseMessage response = await Client.GetAsync(requestUri: requestUri);
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: content);

        int actualStatusCode = (int)response.StatusCode;

        // Then
        actualStatusCode.Should()
            .Be(expected: (int)HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("/Api/AppSecurity/$metadata")]
    [InlineData("/Api/ContentManagement/$metadata")]
    [InlineData("/Api/DocumentManagement/$metadata")]
    [InlineData("/Api/Logging/$metadata")]
    [InlineData("/Api/Mail/$metadata")]
    [InlineData("/Api/Packaging/$metadata")]
    [InlineData("/Api/Security/$metadata")]
    [InlineData("/Api/Workflow/$metadata")]
    public async Task Invoke_ReturnsMetadataDocument(
        string requestUri)
    {
        // Given

        // When
        using HttpResponseMessage response = await Client.GetAsync(requestUri: requestUri);
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: content);

        int actualStatusCode = (int)response.StatusCode;

        // Then
        actualStatusCode.Should()
            .Be(expected: (int)HttpStatusCode.OK);
    }
}