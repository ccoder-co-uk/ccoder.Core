using System.Text.Json;
using cCoder.Core.Exposures.Setup;
using cCoder.Data.Models.CMS;
using FluentAssertions;
using Web.AcceptanceTests.Infrastructure;
using Xunit;


namespace Web.AcceptanceTests.Tests;

public sealed class BaselineAssetTests
{
    [Theory]
    [InlineData("Core.Resource.latest.json")]
    [InlineData("Core.Component.latest.json")]
    [InlineData("Core.Script.latest.json")]
    public void Common_cache_assets_are_present_and_non_empty(string fileName)
    {
        using var json = AcceptanceAssetLoader.LoadJson(fileName);

        json.RootElement.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
        json.RootElement.GetRawText().Length.Should().BeGreaterThan(2);
    }

    [Fact]
    public void App_export_asset_is_present_and_contains_items()
    {
        using var json = AcceptanceAssetLoader.LoadJson("App.1.Export.json");

        json.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        json.RootElement.TryGetProperty("value", out var value).Should().BeTrue();
        value.ValueKind.Should().Be(JsonValueKind.Array);
        value.GetArrayLength().Should().BeGreaterThan(0);
        value[0].TryGetProperty("Items", out var items).Should().BeTrue();
        items.ValueKind.Should().Be(JsonValueKind.Array);
        items.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public void Baseline_packages_include_domain_owned_packages()
    {
        BaselineAssetCatalog catalog = new();
        string[] packageNames = catalog.LoadPackages()
            .Select(package => package.Name)
            .ToArray();

        packageNames.Should().Contain(
        [
            "AppSecurity Components",
            "Content Management Components",
            "Document Management Components",
            "Logging Components",
            "Mail Components",
            "Workflow Components",
        ]);
    }

    [Fact]
    public void Core_review_baseline_only_contains_unresolved_crm_review_items()
    {
        BaselineAssetCatalog catalog = new();
        string[] packageNames = catalog.LoadCoreReviewPackages()
            .Select(package => package.Name)
            .ToArray();

        packageNames.Should().BeEquivalentTo(
        [
            "Core Review Components",
            "Core Review Pages",
            "Core Review Resources",
        ]);

        Component[] components = catalog.LoadPackageItems<Component>("Core Review Components", "Core/Component");
        Page[] pages = catalog.LoadPackageItems<Page>("Core Review Pages", "Core/Page");
        Resource[] resources = catalog.LoadPackageItems<Resource>("Core Review Resources", "Core/Resource");

        components.Select(component => component.Name).Should().BeEquivalentTo(
        [
            "AppList",
            "Client",
            "ClientFiles",
            "ClientList",
            "ClientState",
            "HistoryList",
            "TenantActivity",
            "TenantAppManagement",
            "TenantDetailsManagement",
            "TenantManagement",
            "TenantThemeManagement",
            "ThemeList",
        ]);

        components.Should().OnlyContain(component =>
            string.Equals(component.Key, "CRM", StringComparison.OrdinalIgnoreCase));

        pages.Select(page => page.Path).Should().BeEquivalentTo(
        [
            "Clients",
            "Clients/Client",
        ]);

        resources.Should().NotBeEmpty();
        resources.Should().OnlyContain(resource =>
            string.Equals(resource.Key, "CRM", StringComparison.OrdinalIgnoreCase));
    }
}
