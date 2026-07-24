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
        // When
        using HttpResponseMessage response = await Client.GetAsync(requestUri: BaseUrl);
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
        // When
        using HttpResponseMessage response = await Client.GetAsync(requestUri: $"{BaseUrl}/$metadata");
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: content);

        int actualStatusCode = (int)response.StatusCode;

        // Then
        actualStatusCode.Should()
            .Be(expected: (int)HttpStatusCode.OK);
    }
}