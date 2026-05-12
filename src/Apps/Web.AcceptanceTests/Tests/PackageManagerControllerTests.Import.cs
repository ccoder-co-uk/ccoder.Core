using System.Net;
using System.Text;
using System.Text.Json;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Packaging;
using FluentAssertions;
using FluentAssertions.Execution;
using Web.AcceptanceTests.Infrastructure;
using Xunit;
using CoreApp = cCoder.Data.Models.CMS.App;


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

    [Fact]
    public async Task ShouldRoundTripCapturedPackagesIntoNewApp()
    {
        CoreApp created = await AddAppAsync(new CoreApp
        {
            Name = Unique("Imported Target"),
            Domain = $"{Guid.NewGuid():N}.local",
            TenantId = Unique("tenant"),
            DefaultTheme = "Default",
            DefaultCultureId = string.Empty,
            ConfigJson = "{\"deployment\":{\"dms\":[\"Content\"]}}",
        });

        Package[] capturedPackages = AcceptanceSeedData.LoadExportPackages();

        await ImportPackagesAsync(created.Id, capturedPackages);

        IReadOnlyList<Package> exportedPackages = await ExportPackagesAsync(created.Id);

        using AssertionScope _ = new();

        foreach (object[] row in CapturedPackageTypeCounts())
        {
            string packageName = (string)row[0];
            string itemType = (string)row[1];
            int expectedCount = (int)row[2];

            CountComparableExportedEntities(exportedPackages, packageName, itemType)
                .Should()
                .Be(expectedCount, $"{packageName} should round-trip {itemType} items");
        }
    }

    [Fact]
    public async Task ShouldPreserveCapturedCustomPagePathsWhenImportedIntoNewApp()
    {
        CoreApp created = await AddAppAsync(new CoreApp
        {
            Name = Unique("Imported Target"),
            Domain = $"{Guid.NewGuid():N}.local",
            TenantId = Unique("tenant"),
            DefaultTheme = "Default",
            DefaultCultureId = string.Empty,
            ConfigJson = "{\"deployment\":{\"dms\":[\"Content\"]}}",
        });

        await ImportPackagesAsync(created.Id, AcceptanceSeedData.LoadExportPackages());

        IReadOnlyList<Package> exportedPackages = await ExportPackagesAsync(created.Id);
        PackageItem[] pageItems = exportedPackages
            .Where(found => string.Equals(found.Name, "Pages", StringComparison.OrdinalIgnoreCase))
            .SelectMany(found => found.Items ?? [])
            .Where(found => string.Equals(found.Type, "Core/Page", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        bool hasCommonCachePage = pageItems.Any(item =>
        {
            using JsonDocument document = JsonDocument.Parse(item.Data);

            return document.RootElement.ValueKind == JsonValueKind.Array
                && document.RootElement.EnumerateArray().Any(page =>
                    string.Equals(page.GetProperty("Path").GetString(), "Admin/CommonCache", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(page.GetProperty("Name").GetString(), "Common Cache Endpoint", StringComparison.Ordinal));
        });

        hasCommonCachePage.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldImportAppConfigurationWithoutOverwritingLocalDomainOrTenant()
    {
        CoreApp created = await AddAppAsync(new CoreApp
        {
            Name = Unique("Target App"),
            Domain = $"{Guid.NewGuid():N}.local",
            TenantId = Unique("tenant"),
            DefaultTheme = "Default",
            DefaultCultureId = string.Empty,
            ConfigJson = "{\"deployment\":{\"dms\":[\"Content\"]}}",
        });

        string originalDomain = created.Domain;
        string originalTenantId = created.TenantId;
        string body = JsonSerializer.Serialize(
            new
            {
                name = "AppConfiguration",
                description = "Acceptance app configuration package",
                category = "Acceptance",
                sourceApi = "https://acceptance.local",
                items = new[]
                {
                    new
                    {
                        type = "Core/App",
                        data = JsonSerializer.Serialize(
                            new
                            {
                                Name = "Imported App",
                                Domain = "live.example.com",
                                TenantId = "live-tenant",
                                DefaultTheme = "Ocean",
                                DefaultCultureId = "en-GB",
                                ConfigJson = "{\"deployment\":{\"dms\":[\"Content\",\"Common/Cache\"]}}",
                            }),
                    },
                },
            });

        int statusCode = await ImportPackageAsync(body, created.Id);

        CoreApp updated = await GetStoredAppAsync(created.Id);

        statusCode.Should().Be((int)HttpStatusCode.OK);
        updated.Name.Should().Be("Imported App");
        updated.DefaultTheme.Should().Be("Ocean");
        updated.DefaultCultureId.Should().Be("en-GB");
        updated.ConfigJson.Should().Contain("Common/Cache");
        updated.Domain.Should().Be(originalDomain);
        updated.TenantId.Should().Be(originalTenantId);
    }
}


