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

        IReadOnlyList<Package> actualPackages = await ExportPackagesAsync(1);

        actualPackages.Should().HaveCountGreaterThan(5);
        actualPackages
            .Select(package => package.Name)
            .Should()
            .Contain(expectedPackages.Select(package => package.Name).Distinct());
    }

    [Fact]
    public async Task ShouldExportExpectedEntityCountsForEachCapturedPackageType()
    {
        var created = await AddAppAsync(new cCoder.Data.Models.CMS.App
        {
            Name = Unique("Export Target"),
            Domain = $"{Guid.NewGuid():N}.local",
            TenantId = Unique("tenant"),
            DefaultTheme = "Default",
            DefaultCultureId = string.Empty,
            ConfigJson = "{\"deployment\":{\"dms\":[\"Content\"]}}",
        });

        await ImportPackagesAsync(created.Id, AcceptanceSeedData.LoadExportPackages());

        IReadOnlyList<Package> actualPackages = await ExportPackagesAsync(created.Id);

        using AssertionScope _ = new();

        foreach (object[] row in CapturedPackageTypeCounts())
        {
            string packageName = (string)row[0];
            string itemType = (string)row[1];
            int expectedCount = (int)row[2];

            CountComparableExportedEntities(actualPackages, packageName, itemType)
                .Should()
                .Be(expectedCount, $"{packageName} should export its {itemType} items");
        }
    }

    [Fact]
    public async Task ShouldIncludeAppConfigurationPackageWhenExport()
    {
        var expectedApp = await GetStoredAppAsync(1);
        IReadOnlyList<Package> packages = await ExportPackagesAsync(1);

        Package appConfiguration = packages.Single(found =>
            string.Equals(found.Name, "AppConfiguration", StringComparison.OrdinalIgnoreCase));

        appConfiguration.Items.Should().ContainSingle(found =>
            string.Equals(found.Type, "Core/App", StringComparison.OrdinalIgnoreCase));

        using JsonDocument document = JsonDocument.Parse(appConfiguration.Items.Single().Data);
        document.RootElement.GetProperty("Name").GetString().Should().Be(expectedApp.Name);
        document.RootElement.GetProperty("Domain").GetString().Should().Be(expectedApp.Domain);
        document.RootElement.GetProperty("TenantId").GetString().Should().Be(expectedApp.TenantId);
    }
}

