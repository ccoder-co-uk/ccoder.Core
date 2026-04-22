using System.Text.Json;
using System.Text.RegularExpressions;
using cCoder.Core.Setup;
using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Web.AcceptanceTests.Infrastructure;
using Xunit;


namespace Web.AcceptanceTests.Tests;

public sealed class BaselineAssetTests
{
    private static readonly Regex[] ComponentReferencePatterns =
    [
        new(@"\[component\[([^\]]+)\]\]", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"loadComponent\s*\(\s*['""]([^'""]+)['""]", RegexOptions.IgnoreCase | RegexOptions.Compiled),
    ];
    private static readonly Regex MetaReferencePattern =
        new(@"\[meta\[([^\]]+)\]\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ImageSourcePattern =
        new(@"<img[^>]+src=[""'](?<src>[^""']+)[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
    public void First_time_setup_components_include_common_cache_baseline_dependencies()
    {
        BaselineAssetCatalog catalog = new();
        Component[] components = catalog.LoadPackageItems<Component>("Components", "Core/Component");

        Component topNav = components.Single(component => component.Name == "TopNav");
        topNav.Script.Should().Contain("ContentManagement/Page?$filter=AppId eq ");
        topNav.Script.Should().Contain("ParentId eq null and ShowOnMenus eq true");
        topNav.Script.Should().Contain("$filter=ShowOnMenus eq true");
        topNav.Script.Should().Contain("$orderby=Order asc");
        topNav.Script.Should().Contain("$expand=PageInfo,Pages(");
        topNav.Script.Should().Contain("submenu dropdown-menu");
        topNav.Script.Should().NotContain("__allPages");

        components.Select(component => component.Name).Should().Contain(
        [
            "DetailedNav",
            "CultureManagement",
            "LogStream",
            "RolePrivManagement",
            "MailManagement",
            "FolderManagement",
        ]);
    }

    [Fact]
    public void First_time_setup_components_cache_metadata_under_endpoint_namespaces()
    {
        BaselineAssetCatalog catalog = new();
        Component[] components = catalog.LoadPackageItems<Component>("Components", "Core/Component");

        Component cms = components.Single(component => component.Name == "CMS");
        cms.Script.Should().Contain("\"Name\": \"Core\"");
        cms.Script.Should().Contain("[meta[Core/Page]]");
        cms.Script.Should().Contain("[meta[Core/Layout]]");
        cms.Script.Should().NotContain("[meta[ContentManagement/Page]]");

        Component componentManagement = components.Single(component => component.Name == "ComponentManagement");
        componentManagement.Script.Should().Contain("\"Name\": \"ContentManagement\"");
        componentManagement.Script.Should().Contain("[meta[Core/Component]]");
        componentManagement.Script.Should().NotContain("[meta[ContentManagement/Component]]");

        Component workflowManagement = components.Single(component => component.Name == "WorkflowManagement");
        workflowManagement.Script.Should().Contain("\"Name\": \"Workflow\"");
        workflowManagement.Script.Should().Contain("[meta[Core/FlowDefinition]]");
        workflowManagement.Script.Should().NotContain("[meta[Workflow/FlowDefinition]]");

        Component scheduling = components.Single(component => component.Name == "Scheduling");
        scheduling.Script.Should().Contain("\"Name\": \"Core\"");
        scheduling.Script.Should().Contain("[meta[Core/ScheduledTask]]");
        scheduling.Script.Should().Contain("[meta[Core/FlowDefinition]]");
        scheduling.Script.Should().NotContain("[meta[Scheduling/ScheduledTask]]");
        scheduling.Script.Should().NotContain("[meta[Workflow/FlowDefinition]]");

        Component logStream = components.Single(component => component.Name == "LogStream");
        logStream.Script.Should().Contain("session.apiRoot + \"Hubs/Logs\"");
        logStream.Script.Should().NotContain("withUrl(\"/Hubs/Logs\")");

        Component roleManagement = components.Single(component => component.Name == "RoleManagement");
        roleManagement.Script.Should().Contain("\"Name\": \"AppSecurity\"");
        roleManagement.Script.Should().Contain("[meta[Core/Role]]");
        roleManagement.Script.Should().NotContain("[meta[AppSecurity/Role]]");

        Component commonCache = components.Single(component => component.Name == "CommonCacheEndpoint");
        commonCache.Script.Should().Contain("\"Name\": \"Core\"");
        commonCache.Script.Should().Contain("[meta[Core/Component]]");
        commonCache.Script.Should().Contain(".component[name=CommonCacheEndpoint]");
        commonCache.Script.Should().NotContain("\"Name\": \"CommonCache\"");
        commonCache.Script.Should().NotContain("[meta[ContentManagement/Component]]");

        Component commonCacheComponents = components.Single(component => component.Name == "CommonCacheComponents");
        commonCacheComponents.Script.Should().Contain("type=Core/Component");
        commonCacheComponents.Script.Should().NotContain("type=ContentManagement/Component");

        Component appManagement = components.Single(component => component.Name == "AppManagement");
        appManagement.Content.Should().NotContain("TestimonialManagement");
        appManagement.Content.Should().NotContain("testimonialmanagement");

        Component detailedNav = components.Single(component => component.Name == "DetailedNav");
        detailedNav.Script.Should().Contain("Core/Page?$filter=AppId eq ");

        Component sideNav = components.Single(component => component.Name == "Sidenav");
        sideNav.Content.Should().Contain("documentationTree");
        sideNav.Content.Should().NotContain("[navExpanded[");
        sideNav.Script.Should().Contain("ContentManagement/Page?$filter=AppId eq ");
        sideNav.Script.Should().Contain("ShowOnMenus eq true");

        string[] metadataReferences = components
            .SelectMany(component => MetaReferencePattern
                .Matches(component.Script ?? string.Empty)
                .Select(match => match.Groups[1].Value))
            .ToArray();

        metadataReferences.Should().NotContain(reference =>
            reference.StartsWith("ContentManagement/", StringComparison.OrdinalIgnoreCase)
            || reference.StartsWith("AppSecurity/", StringComparison.OrdinalIgnoreCase)
            || reference.StartsWith("Scheduling/", StringComparison.OrdinalIgnoreCase)
            || string.Equals(reference, "Workflow/FlowDefinition", StringComparison.OrdinalIgnoreCase)
            || string.Equals(reference, "Workflow/FlowInstanceData", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void First_time_setup_layouts_use_packaged_dms_company_logo()
    {
        BaselineAssetCatalog catalog = new();
        Layout[] layouts = catalog.LoadPackageItems<Layout>("Layouts", "Core/Layout");
        byte[] logoBytes = catalog.LoadAssetBytes("Baseline/DMS/Content/CompanyLogoTransparent.png");

        logoBytes.Take(8).Should().Equal(137, 80, 78, 71, 13, 10, 26, 10);
        layouts.Should().OnlyContain(layout =>
            (layout.Html ?? string.Empty).Contains("[app[root]]Api/DMS/Content/CompanyLogoTransparent.png"));
        layouts.Should().OnlyContain(layout =>
            (layout.Html ?? string.Empty).Contains("class=\"header-logo\""));
        layouts.Should().OnlyContain(layout =>
            !(layout.Html ?? string.Empty).Contains("class=\"site-logo\"")
            && !(layout.Html ?? string.Empty).Contains("font-size: 2rem; font-weight: 700;")
            && !(layout.Html ?? string.Empty).Contains("max-height:72px"));
    }

    [Fact]
    public void First_time_setup_documentation_images_use_packaged_dms_assets()
    {
        BaselineAssetCatalog catalog = new();
        Page[] pages = catalog.LoadPackageItems<Page>("Pages", "Core/Page");
        HashSet<string> dmsAssets = catalog.LoadDmsAssetPaths().ToHashSet(StringComparer.OrdinalIgnoreCase);

        string[] imageSources = pages
            .SelectMany(page => page.Contents ?? [])
            .SelectMany(content => ImageSourcePattern
                .Matches(content.Html ?? string.Empty)
                .Select(match => match.Groups["src"].Value))
            .Where(source => !source.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        imageSources.Should().NotBeEmpty();
        imageSources.Should().OnlyContain(source =>
            !source.StartsWith("https://dev.corporatelinx.com", StringComparison.OrdinalIgnoreCase));
        imageSources.Should().OnlyContain(source =>
            !source.StartsWith("/Api/DMS/Content", StringComparison.OrdinalIgnoreCase));

        string[] packagedDmsSources = imageSources
            .Where(source => source.StartsWith("[app[root]]Api/DMS/Content/", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        packagedDmsSources.Should().NotBeEmpty();

        foreach (string source in packagedDmsSources)
        {
            string assetPath = $"Baseline/DMS/Content/{source["[app[root]]Api/DMS/Content/".Length..]}";
            string matchingAssetPath = dmsAssets.Single(path =>
                string.Equals(path, assetPath, StringComparison.OrdinalIgnoreCase));
            catalog.LoadAssetBytes(matchingAssetPath).Should().NotBeEmpty();
        }
    }

    [Fact]
    public void First_time_setup_folder_roles_exclude_source_portal_test_and_retired_folders()
    {
        BaselineAssetCatalog catalog = new();
        string[] paths = catalog.LoadPackages()
            .SelectMany(package => package.Items ?? [])
            .Where(item => string.Equals(item.Type, "Core/FolderRole", StringComparison.OrdinalIgnoreCase))
            .SelectMany(item => JArray.Parse(item.Data).OfType<JObject>())
            .Select(role => role.Value<string>("Path"))
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .ToArray();

        paths.Should().Contain("content");
        paths.Should().Contain("icons");
        paths.Should().NotContain(path =>
            path.Contains("brandnew270120", StringComparison.OrdinalIgnoreCase)
            || path.Contains("renamed270120", StringComparison.OrdinalIgnoreCase)
            || path.Contains("folderb", StringComparison.OrdinalIgnoreCase)
            || path.Contains("folderc", StringComparison.OrdinalIgnoreCase)
            || path.Contains("testimonial", StringComparison.OrdinalIgnoreCase));
        paths.Should().NotContain(path =>
            string.Equals(path, "documentation", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("documentation/", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void First_time_setup_common_cache_scripts_include_referenced_core_scripts_only()
    {
        BaselineAssetCatalog catalog = new();
        Script[] scripts = catalog.LoadPackageItems<Script>("Scripts", "Core/Script");

        scripts.Select(script => script.Name).Should().BeEquivalentTo(
        [
            "DefaultResourcing",
            "KendoCultures",
            "MigrateApp",
        ]);

        scripts.Should().OnlyContain(script =>
            !string.Equals(script.Key, "B2B", StringComparison.OrdinalIgnoreCase));
        scripts.Should().OnlyContain(script =>
            !string.IsNullOrWhiteSpace(script.Content));
    }

    [Fact]
    public void First_time_setup_menu_excludes_retired_and_auth_pages()
    {
        BaselineAssetCatalog catalog = new();
        Page[] pages = catalog.LoadPackageItems<Page>("Pages", "Core/Page");

        pages.Select(page => page.Path).Should().NotContain(
        [
            "Tools",
            "Tools/ApiTester",
            "Tools/DataGenerator",
            "Admin/AshPortalAdmin",
        ]);

        pages.Where(page => page.Path is "Login" or "ResetPassword" or "Admin/WorkflowDesigner")
            .Should()
            .OnlyContain(page => !page.ShowOnMenus);

        pages.Single(page => page.Path == string.Empty).ShowOnMenus.Should().BeTrue();

        Page clients = pages.Single(page => page.Path == "Clients");
        clients.ShowOnMenus.Should().BeTrue();
        clients.Contents.Should().Contain(content =>
            string.Equals(content.Html, "[component[TenantManagement]]", StringComparison.OrdinalIgnoreCase));

        Page client = pages.Single(page => page.Path == "Clients/Client");
        client.ShowOnMenus.Should().BeFalse();
        client.Contents.Should().Contain(content =>
            string.Equals(content.Html, "[component[Client]]", StringComparison.OrdinalIgnoreCase));

        Page fullLogStream = pages.Single(page => page.Path == "Admin/FullLogStream");
        fullLogStream.Contents.Should().Contain(content =>
            string.Equals(content.CultureId, string.Empty, StringComparison.Ordinal)
            && string.Equals(content.Html, "[component[LogStream]]", StringComparison.OrdinalIgnoreCase));

        Page businessProcesses = pages.Single(page => page.Path == "Admin/BusinessProcesses");
        businessProcesses.Contents.Should().OnlyContain(content =>
            string.Equals(content.Html, "[component[WorkflowManagement]]", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void First_time_setup_visible_pages_reference_packaged_components()
    {
        BaselineAssetCatalog catalog = new();
        Page[] pages = catalog.LoadPackageItems<Page>("Pages", "Core/Page");
        Layout[] layouts = catalog.LoadPackageItems<Layout>("Layouts", "Core/Layout");
        Component[] components = catalog.LoadPackageItems<Component>("Components", "Core/Component");

        Dictionary<string, Component> componentsByName = components.ToDictionary(
            component => component.Name,
            StringComparer.OrdinalIgnoreCase);
        Dictionary<string, Layout> layoutsByName = layouts.ToDictionary(
            layout => layout.Name,
            StringComparer.OrdinalIgnoreCase);

        HashSet<string> visitedReferences = new(StringComparer.OrdinalIgnoreCase);
        Queue<string> pendingReferences = new();

        foreach (Page page in pages.Where(page => page.ShowOnMenus || page.Path == string.Empty))
        {
            foreach (Content content in page.Contents ?? [])
                EnqueueReferences(content.Html, visitedReferences, pendingReferences);

            if (!string.IsNullOrWhiteSpace(page.Layout)
                && layoutsByName.TryGetValue(page.Layout, out Layout layout))
            {
                EnqueueReferences(layout.HeaderHtml, visitedReferences, pendingReferences);
                EnqueueReferences(layout.Html, visitedReferences, pendingReferences);
                EnqueueReferences(layout.Script, visitedReferences, pendingReferences);
            }
        }

        while (pendingReferences.TryDequeue(out string reference))
        {
            if (!componentsByName.TryGetValue(reference, out Component component))
                continue;

            EnqueueReferences(component.Content, visitedReferences, pendingReferences);
            EnqueueReferences(component.Script, visitedReferences, pendingReferences);
        }

        string[] missingReferences = visitedReferences
            .Where(reference => !componentsByName.ContainsKey(reference))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        missingReferences.Should().BeEmpty();
        visitedReferences.Should().Contain(reference =>
            string.Equals(reference, "DetailedNav", StringComparison.OrdinalIgnoreCase));
        visitedReferences.Should().Contain(reference =>
            string.Equals(reference, "LogStream", StringComparison.OrdinalIgnoreCase));
    }

    private static void EnqueueReferences(
        string text,
        HashSet<string> visitedReferences,
        Queue<string> pendingReferences)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        foreach (Regex pattern in ComponentReferencePatterns)
        {
            foreach (Match match in pattern.Matches(text))
            {
                string reference = match.Groups[1].Value;
                if (visitedReferences.Add(reference))
                    pendingReferences.Enqueue(reference);
            }
        }
    }
}



