using System.Net;
using System.Text;
using System.Text.Json;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Packaging;
using FluentAssertions;
using Xunit;


namespace Web.AcceptanceTests.Tests.Api;

public sealed partial class PackageManagerControllerTests
{
    [Fact]
    public async Task ShouldImportPackageFromBodyWhenImportThis()
    {
        string name = Unique("ImportedPackage");

        using HttpRequestMessage request = new(HttpMethod.Post, $"{BaseUrl}/ImportThis?appId=1")
        {
            Content = new StringContent(
                $$"""
                {
                  "name": "{{name}}",
                  "description": "Acceptance import package",
                  "category": "Acceptance",
                  "sourceApi": "https://acceptance.local",
                  "items": []
                }
                """,
                Encoding.UTF8,
                "application/json"),
        };

        using HttpResponseMessage response = await Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
    }

    [Fact]
    public async Task ShouldImportPackageArrayFromBodyWhenImportThis()
    {
        string name = Unique("ImportedPackages");

        using HttpRequestMessage request = new(HttpMethod.Post, $"{BaseUrl}/ImportThis?appId=1")
        {
            Content = new StringContent(
                $$"""
                [
                  {
                    "name": "{{name}}",
                    "description": "Acceptance import package",
                    "category": "Acceptance",
                    "sourceApi": "https://acceptance.local",
                    "items": []
                  }
                ]
                """,
                Encoding.UTF8,
                "application/json"),
        };

        using HttpResponseMessage response = await Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
    }

    [Fact]
    public async Task ShouldImportResourcesIntoSeededAppWhenImport()
    {
        string uniqueResourceKey = Unique("resource-key");
        Package package = new("Resources")
        {
            Items =
            [
                new PackageItem
                {
                    Type = "Core/Resource",
                    Data = JsonSerializer.Serialize(
                        new[]
                        {
                            new Resource
                            {
                                Name = Unique("ImportedResource"),
                                Key = uniqueResourceKey,
                                Culture = string.Empty,
                                DisplayName = "Imported Resource",
                                ShortDisplayName = "Imported",
                            },
                        }),
                },
            ],
        };

        int statusCode = await ImportPackageAsync(1, package);
        IReadOnlyList<Package> exportedPackages = await ExportPackagesAsync(1);

        statusCode.Should().Be((int)HttpStatusCode.OK);
        exportedPackages
            .Where(found => string.Equals(found.Name, "Resources", StringComparison.OrdinalIgnoreCase))
            .SelectMany(found => found.Items ?? [])
            .Should()
            .Contain(item => item.Data.Contains(uniqueResourceKey, StringComparison.Ordinal));
    }
}


