using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using cCoder.Data.Models.Packaging;
using cCoder.Data.Models.CMS;
using FluentAssertions;
using Web.AcceptanceTests.Infrastructure;
using Xunit;
using CoreApp = cCoder.Data.Models.CMS.App;


namespace Web.AcceptanceTests.Tests.Api;

[Collection(WebAcceptanceCollection.Name)]
public sealed partial class PackageManagerControllerTests(WebAcceptanceFixture fixture)
{
    private HttpClient Client { get; } = fixture.Client;
    private string BaseUrl { get; } = "/Api/Core/Package";
    private string AppBaseUrl { get; } = "/Api/Core/App";
    private static JsonSerializerOptions JsonOptions { get; } = new() { PropertyNameCaseInsensitive = true };

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private async Task<int> ImportPackageAsync(string body)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, $"{BaseUrl}/ImportThis?appId=1")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };

        using HttpResponseMessage response = await Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        return (int)response.StatusCode;
    }

    private async Task<int> ImportPackageAsync(int appId, Package package)
    {
        using HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{BaseUrl}/Import?appId={appId}",
            package);

        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        return (int)response.StatusCode;
    }

    private async Task<IReadOnlyList<Package>> ExportPackagesAsync(int appId)
    {
        using HttpResponseMessage response = await Client.GetAsync($"{BaseUrl}/Export?appId={appId}");
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        return JsonSerializer.Deserialize<List<Package>>(content, JsonOptions)!;
    }

    private async Task<CoreApp> CreateAppAsync(object payload)
    {
        using HttpResponseMessage response = await Client.PostAsJsonAsync(AppBaseUrl, payload);
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        return JsonSerializer.Deserialize<CoreApp>(content, JsonOptions)!;
    }

    private async Task DeleteAppAsync(int appId)
    {
        using HttpResponseMessage response = await Client.DeleteAsync($"{AppBaseUrl}({appId})");
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
    }

    private static int CountExportedItems(IReadOnlyList<Package> packages, string packageName, string itemType) =>
        packages
            .Where(package => string.Equals(package.Name, packageName, StringComparison.OrdinalIgnoreCase))
            .SelectMany(package => package.Items ?? [])
            .Count(item => string.Equals(item.Type, itemType, StringComparison.OrdinalIgnoreCase));
}


