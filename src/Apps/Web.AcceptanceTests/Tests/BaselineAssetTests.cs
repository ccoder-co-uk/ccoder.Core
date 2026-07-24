// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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
        using var json = AcceptanceAssetLoader.LoadJson(fileName: fileName);

        json.RootElement.ValueKind.Should()
            .BeOneOf(JsonValueKind.Array, JsonValueKind.Object);

        json.RootElement.GetRawText().Length.Should()
            .BeGreaterThan(expected: 2);
    }

    [Fact]
    public void App_export_asset_is_present_and_contains_items()
    {
        using var json = AcceptanceAssetLoader.LoadJson(fileName: "App.1.Export.json");

        json.RootElement.ValueKind.Should()
            .Be(expected: JsonValueKind.Object);

        json.RootElement.TryGetProperty(propertyName: "value",value: out var value)
            .Should()
            .BeTrue();

        value.ValueKind.Should()
            .Be(expected: JsonValueKind.Array);

        value.GetArrayLength()
            .Should()
            .BeGreaterThan(expected: 0);

        value[0].TryGetProperty(propertyName: "Items",value: out var items)
            .Should()
            .BeTrue();

        items.ValueKind.Should()
            .Be(expected: JsonValueKind.Array);

        items.GetArrayLength()
            .Should()
            .BeGreaterThan(expected: 0);
    }

    [Fact]
    public void Baseline_packages_include_domain_owned_packages()
    {
        BaselineAssetCatalog catalog = new();

        string[] packageNames = catalog.LoadPackages()
            .Select(selector: package => package.Name)
            .ToArray();

        packageNames.Should()
            .Contain(
expected:         [
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
            .Select(selector: package => package.Name)
            .ToArray();

        packageNames.Should()
            .BeEquivalentTo(
expectation:         [
            "Core Review Components",
            "Core Review Pages",
            "Core Review Resources",
        ]);

        Component[] components = catalog.LoadPackageItems<Component>(packageName: "Core Review Components",itemType: "Core/Component");
        Page[] pages = catalog.LoadPackageItems<Page>(packageName: "Core Review Pages",itemType: "Core/Page");
        Resource[] resources = catalog.LoadPackageItems<Resource>(packageName: "Core Review Resources",itemType: "Core/Resource");

        components.Select(selector: component => component.Name)
            .Should()
            .BeEquivalentTo(
expectation:         [
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

        components.Should()
            .OnlyContain(predicate: component =>
            string.Equals(a: component.Key,b: "CRM",comparisonType: StringComparison.OrdinalIgnoreCase));

        pages.Select(selector: page => page.Path)
            .Should()
            .BeEquivalentTo(
expectation:         [
            "Clients",
            "Clients/Client",
        ]);

        resources.Should()
            .NotBeEmpty();

        resources.Should()
            .OnlyContain(predicate: resource =>
            string.Equals(a: resource.Key,b: "CRM",comparisonType: StringComparison.OrdinalIgnoreCase));
    }
}