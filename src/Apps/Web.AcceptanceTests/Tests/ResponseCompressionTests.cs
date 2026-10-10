// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Web.AcceptanceTests.Infrastructure;
using Xunit;

namespace Web.AcceptanceTests.Tests;

[Collection(WebAcceptanceCollection.Name)]
public sealed partial class ResponseCompressionTests(WebAcceptanceFixture fixture)
{
    [Fact]
    public async Task LargeResponsesAreCompressedWhenRequested()
    {
        // Given
        using HttpClient client = fixture.CreateMetadataClient();

        using HttpRequestMessage request = new(
            method: HttpMethod.Get,
            requestUri: "/swagger/Core/swagger.json");

        request.Headers.AcceptEncoding.ParseAdd(input: "gzip");

        // When
        using HttpResponseMessage response =
            await client.SendAsync(request: request);

        byte[] content = await response.Content.ReadAsByteArrayAsync();

        // Then
        response.StatusCode
            .Should()
            .Be(expected: HttpStatusCode.OK);

        response.Content.Headers.ContentEncoding
            .Should()
            .Contain(expected: "gzip");

        content.Length
            .Should()
            .BeGreaterThan(expected: 1024);
    }
}