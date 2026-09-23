// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Packaging;
using FluentAssertions;
using FluentAssertions.Execution;
using Web.AcceptanceTests.Infrastructure;
using Xunit;
using System.Text.Json;
using System.Net;

namespace Web.AcceptanceTests.Tests.Api;

public sealed partial class PackageManagerControllerTests
{
    [Fact]
    public async Task LegacyCorePackageExportRoute_WhenRequested_IsNotFound()
    {
        // Given
        string requestUri = "/Api/Core/Package/Export?appId=1";

        // When
        using HttpResponseMessage response = await Client.GetAsync(
            requestUri: requestUri);

        // Then
        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnSeededPackagesWhenExport()
    {
        // Given
        Package[] expectedPackages = AcceptanceSeedData.LoadExportPackages();

        // When
        IReadOnlyList<Package> actualPackages = await ExportCanonicalPackagesAsync(appId: 1);

        // Then
        actualPackages.Should()
            .HaveCountGreaterThan(expected: 5);

        actualPackages
            .Select(selector: package => package.Name)
            .Should()
            .Contain(expected: expectedPackages.Select(selector: package => package.Name)
            .Distinct());

    }

    [Fact]
    public async Task ShouldIncludeAppConfigurationPackageWhenExport()
    {
        // Given
        var expectedApp = await GetStoredAppAsync(appId: 1);

        // When
        IReadOnlyList<Package> packages = await ExportCanonicalPackagesAsync(appId: 1);

        Package appConfiguration = packages.Single(predicate: found =>
            string.Equals(a: found.Name,b: "AppConfiguration",comparisonType: StringComparison.OrdinalIgnoreCase));

        appConfiguration.Items.Should()
            .ContainSingle(predicate: found =>
            string.Equals(a: found.Type,b: "Core/App",comparisonType: StringComparison.OrdinalIgnoreCase));

        using JsonDocument document = JsonDocument.Parse(json: appConfiguration.Items.Single().Data);

        // Then
        document.RootElement.GetProperty(propertyName: "Name")
            .GetString()
            .Should()
            .Be(expected: expectedApp.Name);

        document.RootElement.GetProperty(propertyName: "Domain")
            .GetString()
            .Should()
            .Be(expected: expectedApp.Domain);

        document.RootElement.GetProperty(propertyName: "TenantId")
            .GetString()
            .Should()
            .Be(expected: expectedApp.TenantId);
    }
}