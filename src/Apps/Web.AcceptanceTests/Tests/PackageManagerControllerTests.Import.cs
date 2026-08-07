// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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
        // Given
        string name = Unique(prefix: "ImportedPackage");

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

        // When
        using HttpResponseMessage response = await Client.SendAsync(request: request);
        string content = await response.Content.ReadAsStringAsync();

        // Then
        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: content);
    }

    [Fact]
    public async Task ShouldImportPackageArrayFromBodyWhenImportThis()
    {
        // Given
        string name = Unique(prefix: "ImportedPackages");

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

        // When
        using HttpResponseMessage response = await Client.SendAsync(request: request);
        string content = await response.Content.ReadAsStringAsync();

        // Then
        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: content);
    }

    [Fact]
    public async Task ShouldImportResourcesIntoSeededAppWhenImport()
    {
        // Given
        string uniqueResourceKey = Unique(prefix: "resource-key");

        Package package = new("Resources")
        {
            Items =
            [
                new PackageItem
                {
                    Type = "ContentManagement/Resource",
                    Data = JsonSerializer.Serialize(
value:                         new[]
                        {
                            new Resource
                            {
                                Name = Unique(prefix: "ImportedResource"),
                                Key = uniqueResourceKey,
                                Culture = string.Empty,
                                DisplayName = "Imported Resource",
                                ShortDisplayName = "Imported",
                            },
                        }),
                },
            ],
        };

        // When
        int statusCode = await ImportPackageAsync(appId: 1,package: package);
        IReadOnlyList<Package> exportedPackages = await ExportPackagesAsync(appId: 1);

        // Then
        statusCode.Should()
            .Be(expected: (int)HttpStatusCode.OK);

        exportedPackages
            .Where(predicate: found => string.Equals(a: found.Name,b: "Resources",comparisonType: StringComparison.OrdinalIgnoreCase))
            .SelectMany(selector: found => found.Items ?? [])
            .Should()
            .Contain(predicate: item => item.Data.Contains(value: uniqueResourceKey,comparisonType: StringComparison.Ordinal));
    }

    [Fact]
    public void ShouldContainCapturedCustomPagePaths()
    {
        // Given
        Package[] capturedPackages = AcceptanceSeedData.LoadExportPackages();

        // When
        PackageItem[] pageItems = [.. capturedPackages
            .Where(predicate: found => string.Equals(a: found.Name,b: "Pages",comparisonType: StringComparison.OrdinalIgnoreCase))
            .SelectMany(selector: found => found.Items ?? [])
            .Where(predicate: found => string.Equals(a: found.Type,b: "ContentManagement/Page",comparisonType: StringComparison.OrdinalIgnoreCase))];

        bool hasCommonCachePage = pageItems.Any(predicate: item =>
        {
            using JsonDocument document = JsonDocument.Parse(json: item.Data);

            return document.RootElement.ValueKind == JsonValueKind.Array
                && document.RootElement.EnumerateArray()
                .Any(predicate: page =>
                    string.Equals(a: page.GetProperty(propertyName: "Path")
                .GetString(),b: "Admin/CommonCache",comparisonType: StringComparison.OrdinalIgnoreCase)
                    && string.Equals(a: page.GetProperty(propertyName: "Name")
                .GetString(),b: "Common Cache Endpoint",comparisonType: StringComparison.Ordinal));
        });

        // Then
        hasCommonCachePage.Should()
            .BeTrue();
    }

    [Fact]
    public async Task ShouldImportAppConfigurationWithoutOverwritingLocalDomainOrTenant()
    {
        // Given
        CoreApp created = await AddAppAsync(app: new CoreApp
        {
            Name = Unique(prefix: "Target App"),
            Domain = $"{Guid.NewGuid():N}.local",
            TenantId = Unique(prefix: "tenant"),
            DefaultTheme = "Default",
            DefaultCultureId = string.Empty,
            ConfigJson = "{\"deployment\":{\"dms\":[\"Content\"]}}",
        });

        string originalDomain = created.Domain;
        string originalTenantId = created.TenantId;

        string body = JsonSerializer.Serialize(
value:             new
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
value:                             new
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

        // When
        int statusCode = await ImportPackageAsync(body: body,appId: created.Id);

        CoreApp updated = await GetStoredAppAsync(appId: created.Id);

        // Then
        statusCode.Should()
            .Be(expected: (int)HttpStatusCode.OK);

        updated.Name.Should()
            .Be(expected: "Imported App");

        updated.DefaultTheme.Should()
            .Be(expected: "Ocean");

        updated.DefaultCultureId.Should()
            .Be(expected: "en-GB");

        updated.ConfigJson.Should()
            .Contain(expected: "Common/Cache");

        updated.Domain.Should()
            .Be(expected: originalDomain);

        updated.TenantId.Should()
            .Be(expected: originalTenantId);
    }
}