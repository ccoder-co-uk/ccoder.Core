using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Web.AcceptanceTests.Infrastructure;
using Xunit;


namespace Web.AcceptanceTests.Tests.Api;

[Collection(WebAcceptanceCollection.Name)]
public sealed partial class ApiRootControllerTests(WebAcceptanceFixture fixture)
{
    private HttpClient Client { get; } = fixture.Client;
    private WebAcceptanceFixture Fixture { get; } = fixture;
    private string BaseUrl { get; } = "/Api";

    private async Task<string> GetAsync()
    {
        using HttpResponseMessage response = await Client.GetAsync(BaseUrl);
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        return content;
    }

    private async Task<string> GetMetadataAsync()
    {
        using HttpResponseMessage response = await Client.GetAsync($"{BaseUrl}/GetMetadata");
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        return content;
    }

    private string[] GetRegisteredRoutes() =>
        Fixture.Factory.Services.GetServices<EndpointDataSource>()
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(ToManifestLine)
            .Where(IsManifestRoute)
            .Where(static line => !line.Contains("GetMetadata", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(line => line, StringComparer.Ordinal)
            .ToArray();

    private static string ToManifestLine(RouteEndpoint endpoint)
    {
        string methods =
            string.Join(
                ",",
                endpoint.Metadata
                    .OfType<HttpMethodMetadata>()
                    .SelectMany(metadata => metadata.HttpMethods)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(method => method, StringComparer.Ordinal)
            );

        if (string.IsNullOrWhiteSpace(methods))
            methods = "ANY";

        return $"{methods} {endpoint.RoutePattern.RawText ?? string.Empty}";
    }

    private static bool IsManifestRoute(string line)
    {
        string route = line[(line.IndexOf(' ') + 1)..];

        return route.StartsWith("/Api", StringComparison.Ordinal)
            || route.StartsWith("Api", StringComparison.Ordinal)
            || string.Equals(route, "Setup", StringComparison.Ordinal)
            || string.Equals(route, "AcceptInvite", StringComparison.Ordinal)
            || string.Equals(route, "{*path}", StringComparison.Ordinal);
    }
}



