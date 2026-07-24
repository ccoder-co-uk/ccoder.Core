// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Packaging;
using FluentAssertions;
using FluentAssertions.Execution;
using Web.AcceptanceTests.Infrastructure;
using Xunit;
using System.Text.Json;

namespace Web.AcceptanceTests.Tests.Api;

public sealed partial class PackageManagerControllerTests
{
    [Fact]
    public async Task ShouldReturnSeededPackagesWhenExport()
    {
        Package[] expectedPackages = AcceptanceSeedData.LoadExportPackages();

        IReadOnlyList<Package> actualPackages = await ExportPackagesAsync(appId: 1);

        actualPackages.Should()
            .HaveCountGreaterThan(expected: 5);

        actualPackages
            .Select(selector: package => package.Name)
            .Should()
            .Contain(expected: expectedPackages.Select(selector: package => package.Name)
            .Distinct());
    }

    [Fact]
    public async Task ShouldExportExpectedEntityCountsForEachCapturedPackageType()
    {
        var created = await AddAppAsync(app: new cCoder.Data.Models.CMS.App
        {
            Name = Unique(prefix: "Export Target"),
            Domain = $"{Guid.NewGuid():N}.local",
            TenantId = Unique(prefix: "tenant"),
            DefaultTheme = "Default",
            DefaultCultureId = string.Empty,
            ConfigJson = "{\"deployment\":{\"dms\":[\"Content\"]}}",
        });

        await ImportPackagesAsync(appId: created.Id,packages: AcceptanceSeedData.LoadExportPackages());

        IReadOnlyList<Package> actualPackages = await ExportPackagesAsync(appId: created.Id);

        using AssertionScope _ = new();

        foreach (object[] row in CapturedPackageTypeCounts())
        {
            string packageName = (string)row[0];
            string itemType = (string)row[1];
            int expectedCount = (int)row[2];

            CountComparableExportedEntities(packages: actualPackages,packageName: packageName,itemType: itemType)
                .Should()
                .Be(expected: expectedCount,because: $"{packageName} should export its {itemType} items");
        }
    }

    [Fact]
    public async Task ShouldIncludeAppConfigurationPackageWhenExport()
    {
        var expectedApp = await GetStoredAppAsync(appId: 1);
        IReadOnlyList<Package> packages = await ExportPackagesAsync(appId: 1);

        Package appConfiguration = packages.Single(predicate: found =>
            string.Equals(a: found.Name,b: "AppConfiguration",comparisonType: StringComparison.OrdinalIgnoreCase));

        appConfiguration.Items.Should()
            .ContainSingle(predicate: found =>
            string.Equals(a: found.Type,b: "Core/App",comparisonType: StringComparison.OrdinalIgnoreCase));

        using JsonDocument document = JsonDocument.Parse(json: appConfiguration.Items.Single().Data);

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