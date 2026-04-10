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
        using HttpResponseMessage response = await Client.GetAsync(BaseUrl);
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        int actualStatusCode = (int)response.StatusCode;

        // Then
        actualStatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task Invoke_ReturnsMetadataDocument()
    {
        // When
        using HttpResponseMessage response = await Client.GetAsync($"{BaseUrl}/$metadata");
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        int actualStatusCode = (int)response.StatusCode;

        // Then
        actualStatusCode.Should().Be((int)HttpStatusCode.OK);
    }
}



