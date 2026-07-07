using System.Text.Json;
using System.Text.RegularExpressions;
using cCoder.Core.Exposures.Setup;
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
            "AcceptInvite",
            "InviteUser",
            "PendingUserInvites",
            "UserInvitations",
        ]);
    }

    [Fact]
    public void First_time_setup_components_cache_metadata_under_endpoint_namespaces()
    {
        BaselineAssetCatalog catalog = new();
        Component[] components = catalog.LoadPackageItems<Component>("Components", "Core/Component");

        Component cms = components.Single(component => component.Name == "CMS");
        cms.Script.Should().Contain("\"Name\": \"Core\"");
        cms.Script.Should().Contain("[meta[ContentManagement/Page]]");
        cms.Script.Should().Contain("[meta[ContentManagement/Layout]]");
        cms.Script.Should().NotContain("[meta[Core/Page]]");

        Component componentManagement = components.Single(component => component.Name == "ComponentManagement");
        componentManagement.Script.Should().Contain("\"Name\": \"ContentManagement\"");
        componentManagement.Script.Should().Contain("[meta[ContentManagement/Component]]");
        componentManagement.Script.Should().NotContain("[meta[Core/Component]]");

        Component workflowManagement = components.Single(component => component.Name == "WorkflowManagement");
        workflowManagement.Script.Should().Contain("\"Name\": \"Workflow\"");
        workflowManagement.Script.Should().Contain("[meta[Workflow/FlowDefinition]]");
        workflowManagement.Script.Should().NotContain("[meta[Core/FlowDefinition]]");

        Component scheduling = components.Single(component => component.Name == "Scheduling");
        scheduling.Script.Should().Contain("\"Name\": \"Core\"");
        scheduling.Script.Should().Contain("[meta[Workflow/ScheduledTask]]");
        scheduling.Script.Should().Contain("[meta[Workflow/FlowDefinition]]");
        scheduling.Script.Should().NotContain("[meta[Core/ScheduledTask]]");
        scheduling.Script.Should().NotContain("[meta[Core/FlowDefinition]]");
        scheduling.Script.Should().NotContain("[meta[Scheduling/ScheduledTask]]");

        Component logStream = components.Single(component => component.Name == "LogStream");
        logStream.Script.Should().Contain("session.apiRoot + \"Hubs/Logs\"");
        logStream.Script.Should().NotContain("withUrl(\"/Hubs/Logs\")");

        Component roleManagement = components.Single(component => component.Name == "RoleManagement");
        roleManagement.Script.Should().Contain("\"Name\": \"AppSecurity\"");
        roleManagement.Script.Should().Contain("[meta[AppSecurity/Role]]");
        roleManagement.Script.Should().NotContain("[meta[Core/Role]]");

        Component commonCache = components.Single(component => component.Name == "CommonCacheEndpoint");
        commonCache.Script.Should().Contain("\"Name\": \"Core\"");
        commonCache.Script.Should().Contain("[meta[ContentManagement/Component]]");
        commonCache.Script.Should().Contain(".component[name=CommonCacheEndpoint]");
        commonCache.Script.Should().NotContain("\"Name\": \"CommonCache\"");
        commonCache.Script.Should().NotContain("[meta[Core/Component]]");

        Component commonCacheComponents = components.Single(component => component.Name == "CommonCacheComponents");
        commonCacheComponents.Script.Should().Contain("type=ContentManagement/Component");
        commonCacheComponents.Script.Should().NotContain("type=Core/Component");

        Component appManagement = components.Single(component => component.Name == "AppManagement");
        appManagement.Content.Should().NotContain("TestimonialManagement");
        appManagement.Content.Should().NotContain("testimonialmanagement");

        Component register = components.Single(component => component.Name == "Register");
        register.Script.Should().Contain("Account/Register");
        register.Script.Should().NotContain("api.register");
        register.Script.Should().NotContain("api.login(loginModel.Email");

        Component acceptInvite = components.Single(component => component.Name == "AcceptInvite");
        acceptInvite.Key.Should().Be("Account");
        acceptInvite.ResourceKey.Should().Be("Account");
        acceptInvite.Script.Should().Contain("Account/AcceptInvite");
        acceptInvite.Script.Should().NotContain("Security/SSOUser/AcceptInvite");
        acceptInvite.Script.Should().NotContain("B2B");

        Component inviteUser = components.Single(component => component.Name == "InviteUser");
        inviteUser.Key.Should().Be("Account Management");
        inviteUser.ResourceKey.Should().Be("Account");
        inviteUser.Script.Should().Contain("Account/Invite");
        inviteUser.Script.Should().NotContain("B2B");

        Component pendingUserInvites = components.Single(component => component.Name == "PendingUserInvites");
        pendingUserInvites.Script.Should().Contain("Account/ResendInvite");
        pendingUserInvites.Script.Should().NotContain("B2B");

        Component detailedNav = components.Single(component => component.Name == "DetailedNav");
        detailedNav.Script.Should().Contain("ContentManagement/Page?$filter=AppId eq ");

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
            reference.StartsWith("Core/", StringComparison.OrdinalIgnoreCase)
            || reference.StartsWith("Scheduling/", StringComparison.OrdinalIgnoreCase));
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
            .SelectMany(item => ParsePackageItemObjects(item.Data))
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

    private static IEnumerable<JObject> ParsePackageItemObjects(string data)
    {
        JToken token = JToken.Parse(data);

        return token is JArray array
            ? array.OfType<JObject>()
            : token is JObject item
                ? [item]
                : [];
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

        Script migrateApp = scripts.Single(script => script.Name == "MigrateApp");
        migrateApp.Content.Should().Contain("ContentManagement/App(");
        migrateApp.Content.Should().Contain("ContentManagement/App?");
        migrateApp.Content.Should().Contain("Packaging/Package/ImportThis");
        migrateApp.Content.Should().Contain("DocumentManagement/Folder?");
        migrateApp.Content.Should().Contain("DocumentManagement/File?");
        migrateApp.Content.Should().NotContain("Core/App");
        migrateApp.Content.Should().NotContain("Core/Package");
        migrateApp.Content.Should().NotContain("Core/Folder");
        migrateApp.Content.Should().NotContain("Core/File");
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

        pages.Where(page => page.Path is "Login" or "ResetPassword" or "AcceptInvite" or "Admin/WorkflowDesigner")
            .Should()
            .OnlyContain(page => !page.ShowOnMenus);

        Page acceptInvite = pages.Single(page => page.Path == "AcceptInvite");
        acceptInvite.Contents.Should().Contain(content =>
            string.Equals(content.Html, "[component[AcceptInvite]]", StringComparison.OrdinalIgnoreCase));

        Page userInvitations = pages.Single(page => page.Path == "Admin/UserInvitations");
        userInvitations.ShowOnMenus.Should().BeTrue();
        userInvitations.Contents.Should().Contain(content =>
            string.Equals(content.Html, "[component[UserInvitations]]", StringComparison.OrdinalIgnoreCase));

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



