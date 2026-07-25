// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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
        using HttpResponseMessage response = await Client.GetAsync(requestUri: BaseUrl);
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: content);

        return content;
    }

    private async Task<string> GetMetadataAsync()
    {
        using HttpResponseMessage response = await Client.GetAsync(requestUri: $"{BaseUrl}/GetMetadata");
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: content);

        return content;
    }

    private string[] GetRegisteredRoutes() =>
        [.. Fixture.Factory.Services.GetServices<EndpointDataSource>()
            .SelectMany(selector: source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(selector: ToManifestLine)
            .Where(predicate: IsManifestRoute)
            .Where(predicate: static line => !line.Contains(value: "GetMetadata",comparisonType: StringComparison.OrdinalIgnoreCase))
            .Distinct(comparer: StringComparer.Ordinal)
            .OrderBy(keySelector: line => line,comparer: StringComparer.Ordinal)];

    private static string ToManifestLine(RouteEndpoint endpoint)
    {
        string methods =
            string.Join(
separator:                 ",",values:                 endpoint.Metadata
                    .OfType<HttpMethodMetadata>()
                    .SelectMany(selector: metadata => metadata.HttpMethods)
                    .Distinct(comparer: StringComparer.Ordinal)
                    .OrderBy(keySelector: method => method,comparer: StringComparer.Ordinal)
            );

        if (string.IsNullOrWhiteSpace(value: methods))
        {
            methods = "ANY";
        }

        return $"{methods} {endpoint.RoutePattern.RawText ?? string.Empty}";
    }

    private static bool IsManifestRoute(string line)
    {
        string route = line[(line.IndexOf(value: ' ') + 1)..];

        return route.StartsWith(value: "/Api",comparisonType: StringComparison.Ordinal)
            || route.StartsWith(value: "Api",comparisonType: StringComparison.Ordinal)
            || string.Equals(a: route,b: "Setup",comparisonType: StringComparison.Ordinal)
            || string.Equals(a: route,b: "{*path}",comparisonType: StringComparison.Ordinal);
    }
}