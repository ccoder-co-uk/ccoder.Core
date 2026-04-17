using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
using FluentAssertions;
using Web.AcceptanceTests.Infrastructure;
using Xunit;


namespace Web.AcceptanceTests.Tests;

public sealed class BaselineAssetTests
{
    [Fact]
    public void Common_cache_seed_data_can_be_derived_from_the_split_baseline()
    {
        CommonObject[] commonObjects = AcceptanceSeedData.LoadCommonObjects();

        commonObjects.Should().NotBeEmpty();
        commonObjects.Should().OnlyContain(found =>
            found.Type == "Core/Resource"
            || found.Type == "Core/Component"
            || found.Type == "Core/Script");
    }

    [Fact]
    public void Baseline_manifest_can_be_loaded_and_contains_items()
    {
        var packages = AcceptanceSeedData.LoadExportPackages();

        packages.Should().NotBeEmpty();
        packages.Should().OnlyContain(package => package.Items != null && package.Items.Count > 0);
        packages.Select(package => package.Name).Should().Contain(["Roles", "Layouts", "Pages", "Components"]);
    }

    [Fact]
    public void Baseline_components_keep_the_expected_login_and_navigation_scripts()
    {
        Component[] components = AcceptanceSeedData.LoadPackageItems<Component>("Components", "Core/Component");

        Component login = components.Single(component => component.Name == "Login");
        login.Script.Should().Contain("$(\"[name=pass]\").val(),");
        login.Script.Should().Contain("session.token = api.token;");
        login.Script.Should().Contain("setUrlQueryParameter(newLocation, \"t\", api.token)");

        Component topNav = components.Single(component => component.Name == "TopNav");
        topNav.Script.Should().Contain("ContentManagement/Page?$filter=AppId eq ");
        topNav.Script.Should().Contain("ParentId eq null");
        topNav.Script.Should().Contain("$expand=PageInfo,Pages(");
        topNav.Script.Should().NotContain("__allPages");
    }
}



