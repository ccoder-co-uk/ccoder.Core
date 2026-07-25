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

    [Fact]
    public async Task Invoke_ReturnsMetadataDocument()
    {
        // Given
        string requestUri = $"{BaseUrl}/$metadata";

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