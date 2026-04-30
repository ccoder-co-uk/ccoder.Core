using System.Net;
using System.Net.Http.Headers;
using cCoder.Data;
using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Packaging;
using cCoder.Data.Models.Security;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Exposures;
using cCoder.Security.Objects.Entities;
using cCoder.AppSecurity.Services.Orchestrations;
using cCoder.AppSecurity.Brokers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Web.AcceptanceTests.Infrastructure;
using Web.AcceptanceTests.Models;
using Xunit;
using ContentUser = cCoder.Data.Models.Security.User;

namespace Web.AcceptanceTests.Tests;

public sealed partial class FirstTimeSetupTests
{
    [Fact]
    public async Task ShouldRenderSetupExperienceWhenEnvironmentIsEmpty()
    {
        await using SetupHarness harness = await SetupHarness.CreateAsync();

        using HttpResponseMessage response = await harness.Client.GetAsync("/");
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Welcome to cCoder.Core platform setup");
        content.Should().Contain("Primary domain:");
    }

    [Fact]
    public async Task ShouldCreateTenantAdminAndBaselineAppWhenSetupSubmitted()
    {
        await using SetupHarness harness = await SetupHarness.CreateAsync();

        await SubmitSetupAsync(harness);

        using HttpResponseMessage homeResponse = await harness.Client.GetAsync("/");
        string homeContent = await homeResponse.Content.ReadAsStringAsync();

        homeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        homeContent.Should().Contain("/Api/DMS/Content/CompanyLogoTransparent.png");
        homeContent.Should().Contain("Modern bespoke platforms built for connected businesses.");
        homeContent.Should().Contain("Open Platform");
        homeContent.Should().Contain("Explore the platform");
        homeContent.Should().Contain("Notify me");
        homeContent.Should().NotContain("Further Information");
        homeContent.Should().NotContain("Corporate LinX");
        homeContent.Should().Contain("/everything.min.js");
        homeContent.Should().NotContain("Acceptance Admin");
        homeContent.Should().NotContain("Guest (Login)");

        await using DbContext probeCore = harness.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();
        int homePageId = await probeCore.Set<Page>()
            .IgnoreQueryFilters()
            .Where(found => found.Path == string.Empty)
            .Select(found => found.Id)
            .SingleAsync();

        using HttpRequestMessage appRequest = new(HttpMethod.Get, "/Api/ContentManagement/Page");
        appRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using HttpResponseMessage appResponse = await harness.Client.SendAsync(appRequest);
        string appJson = await appResponse.Content.ReadAsStringAsync();

        appResponse.StatusCode.Should().Be(HttpStatusCode.OK, appJson);
        JsonNode appNode = JsonNode.Parse(appJson)!;
        JsonArray appPages = appNode["value"]?.AsArray() ?? [];
        appPages.Any(page =>
            string.Equals(page?["Path"]?.ToString(), "Admin", StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue();

        using HttpRequestMessage pageExpandRequest =
            new(HttpMethod.Get, $"/Api/ContentManagement/Page({homePageId})?$expand=Contents");
        pageExpandRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using HttpResponseMessage pageExpandResponse = await harness.Client.SendAsync(pageExpandRequest);
        string pageExpandJson = await pageExpandResponse.Content.ReadAsStringAsync();

        pageExpandResponse.StatusCode.Should().Be(HttpStatusCode.OK, pageExpandJson);
        JsonNode pageExpandNode = JsonNode.Parse(pageExpandJson)!;
        JsonArray contents = pageExpandNode["Contents"]?.AsArray() ?? [];
        contents.Should().NotBeEmpty();

        await using DbContext core = harness.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();
        await using DbContext sso = harness.Factory.Services
            .GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(true);

        App app = await core.Set<App>()
            .IgnoreQueryFilters()
            .SingleAsync();
        app.Domain.Should().Be("localhost");
        app.TenantId.Should().Be("acceptance-platform");

        (await core.Set<Page>().IgnoreQueryFilters().CountAsync()).Should().BeGreaterThan(0);
        (await core.Set<Package>().IgnoreQueryFilters().CountAsync()).Should().BeGreaterThan(0);
        (await core.Set<CommonObject>().IgnoreQueryFilters().CountAsync()).Should().BeGreaterThan(0);
        (await core.Set<CommonObject>().IgnoreQueryFilters().CountAsync(found => found.Type == "Core/Script"))
            .Should().BeGreaterThan(0);
        string[] folderPaths = await core.Set<Folder>()
            .IgnoreQueryFilters()
            .Select(found => found.Path)
            .OrderBy(path => path)
            .ToArrayAsync();

        folderPaths.Should().Contain(["content", "icons", "content/documentation"]);
        folderPaths.Should().NotContain(path =>
            path.Contains("brandnew270120", StringComparison.OrdinalIgnoreCase)
            || path.Contains("renamed270120", StringComparison.OrdinalIgnoreCase)
            || path.Contains("folderb", StringComparison.OrdinalIgnoreCase)
            || path.Contains("folderc", StringComparison.OrdinalIgnoreCase)
            || path.Contains("testimonial", StringComparison.OrdinalIgnoreCase));
        folderPaths.Should().NotContain(path =>
            string.Equals(path, "documentation", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("documentation/", StringComparison.OrdinalIgnoreCase));

        Dictionary<string, bool> menuVisibility = await core.Set<Page>()
            .IgnoreQueryFilters()
            .Where(found => found.Path == string.Empty
                || found.Path == "Login"
                || found.Path == "ResetPassword"
                || found.Path == "Admin/WorkflowDesigner")
            .ToDictionaryAsync(found => found.Path, found => found.ShowOnMenus);

        menuVisibility[string.Empty].Should().BeTrue();
        menuVisibility["Login"].Should().BeFalse();
        menuVisibility["ResetPassword"].Should().BeFalse();
        menuVisibility["Admin/WorkflowDesigner"].Should().BeFalse();

        ContentUser user = await core.Set<ContentUser>()
            .IgnoreQueryFilters()
            .SingleAsync(found => found.Id == "admin");
        user.Email.Should().Be("admin@localhost");

        cCoder.Data.Models.DMS.File logoFile = await core.Set<cCoder.Data.Models.DMS.File>()
            .IgnoreQueryFilters()
            .Include(found => found.Contents)
            .SingleAsync(found => found.Path == "content/companylogotransparent.png");
        logoFile.Name.Should().Be("CompanyLogoTransparent.png");
        logoFile.MimeType.Should().Be("image/png");
        logoFile.CreatedBy.Should().Be(user.Id);
        logoFile.Contents.Should().ContainSingle();
        logoFile.Contents.Single().RawData.Take(8).Should().Equal(137, 80, 78, 71, 13, 10, 26, 10);

        cCoder.Data.Models.DMS.File docsImageFile = await core.Set<cCoder.Data.Models.DMS.File>()
            .IgnoreQueryFilters()
            .Include(found => found.Contents)
            .SingleAsync(found =>
                found.Path == "content/documentation/standarduserguide/homepage-en.png");
        docsImageFile.MimeType.Should().Be("image/png");
        docsImageFile.CreatedBy.Should().Be(user.Id);
        docsImageFile.Contents.Should().ContainSingle();
        docsImageFile.Contents.Single().RawData.Should().NotBeEmpty();

        using HttpResponseMessage logoResponse = await harness.Client.GetAsync("/Api/DMS/Content/CompanyLogoTransparent.png");
        byte[] logoResponseBytes = await logoResponse.Content.ReadAsByteArrayAsync();

        logoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        logoResponse.Content.Headers.ContentType?.MediaType.Should().Be("image/png");
        logoResponseBytes.Take(8).Should().Equal(137, 80, 78, 71, 13, 10, 26, 10);

        using HttpResponseMessage docsImageResponse =
            await harness.Client.GetAsync("/Api/DMS/Content/documentation/standarduserguide/homepage-en.png");
        byte[] docsImageResponseBytes = await docsImageResponse.Content.ReadAsByteArrayAsync();

        docsImageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        docsImageResponse.Content.Headers.ContentType?.MediaType.Should().Be("image/png");
        docsImageResponseBytes.Should().NotBeEmpty();

        CommonObject[] commonObjects = await core.Set<CommonObject>()
            .IgnoreQueryFilters()
            .ToArrayAsync();
        commonObjects.Should().OnlyContain(found =>
            found.CreatedBy == user.Id
            && found.LastUpdatedBy == user.Id);

        Role adminRole = await core.Set<Role>()
            .IgnoreQueryFilters()
            .SingleAsync(found => found.AppId == app.Id && found.Name == "Administrators");
        adminRole.Privs.Should().Contain("app_create");

        bool hasAdminLink = await core.Set<UserRole>()
            .IgnoreQueryFilters()
            .AnyAsync(found => found.UserId == user.Id && found.RoleId == adminRole.Id);
        hasAdminLink.Should().BeTrue();

        string[] guestRoleNames = await core.Set<UserRole>()
            .IgnoreQueryFilters()
            .Where(found => found.UserId == "Guest")
            .Join(
                core.Set<Role>().IgnoreQueryFilters(),
                userRole => userRole.RoleId,
                role => role.Id,
                (_, role) => role.Name)
            .OrderBy(name => name)
            .ToArrayAsync();
        guestRoleNames.Should().Equal("Guests");

        string[] appComponentNames = await core.Set<Component>()
            .IgnoreQueryFilters()
            .Where(found => found.AppId == app.Id)
            .Select(found => found.Name)
            .OrderBy(name => name)
            .ToArrayAsync();
        appComponentNames.Should().Contain("CoreManagement");
        appComponentNames.Should().OnlyContain(name =>
            string.Equals(name, "CoreManagement", StringComparison.Ordinal)
            || name.StartsWith("SSO", StringComparison.Ordinal));

        CommonObject topNav = commonObjects.Single(found =>
            found.Type == "Core/Component" && found.Name == "TopNav");
        JsonNode topNavJson = JsonNode.Parse(topNav.Json)!;
        string topNavScript = topNavJson["Script"]?.ToString() ?? string.Empty;
        topNavJson["CreatedBy"]?.ToString().Should().Be(user.Id);
        topNavScript.Should().Contain("ContentManagement/Page?$filter=AppId eq ");
        topNavScript.Should().Contain("ParentId eq null and ShowOnMenus eq true");
        topNavScript.Should().Contain("$orderby=Order asc");
        topNavScript.Should().Contain("$filter=ShowOnMenus eq true");
        topNavScript.Should().Contain("submenu dropdown-menu");
        topNavScript.Should().NotContain("/_navigation/topnav");
        topNavScript.Should().NotContain("buildTree");
        topNavScript.Should().NotContain("__allPages");

        CommonObject login = commonObjects.Single(found =>
            found.Type == "Core/Component" && found.Name == "Login");
        JsonNode loginJson = JsonNode.Parse(login.Json)!;
        string loginScript = loginJson["Script"]?.ToString() ?? string.Empty;
        loginJson["CreatedBy"]?.ToString().Should().Be(user.Id);
        loginScript.Should().Contain("$(\"[name=pass]\").val(),");
        loginScript.Should().Contain("session.token = api.token;");
        loginScript.Should().Contain("Token: getQueryParameter(\"t\")");
        loginScript.Should().Contain("Account/ConfirmEmail");
        loginScript.Should().Contain("window.location.reload();");
        loginScript.Should().NotContain("setUrlQueryParameter");
        loginScript.Should().NotContain("getQueryParameter(\"returnUrl\")");

        Tenant tenant = await sso.Set<Tenant>()
            .IgnoreQueryFilters()
            .SingleAsync(found => found.Id == "acceptance-platform");
        tenant.Name.Should().Be("Acceptance Platform");

        SSOUser ssoUser = await sso.Set<SSOUser>()
            .IgnoreQueryFilters()
            .SingleAsync(found => found.Id == "admin");
        ssoUser.EmailConfirmed.Should().BeTrue();

        bool hasPortalAdminRole = await sso.Set<SSOUserRole>()
            .IgnoreQueryFilters()
            .Include(found => found.Role)
            .AnyAsync(found => found.UserId == "admin" && found.Role.Name == "Administrators");
        hasPortalAdminRole.Should().BeTrue();

        SSORole tenantAdminRole = await sso.Set<SSORole>()
            .IgnoreQueryFilters()
            .SingleAsync(found =>
                found.TenantId == tenant.Id
                && found.UsersArePortalAdmins
                && found.Name == "Administrators");
        tenantAdminRole.Privs.Should().Contain("tenant_read");

        SSORole portalAdminRole = await sso.Set<SSORole>()
            .IgnoreQueryFilters()
            .SingleAsync(found =>
                found.TenantId == null
                && found.UsersArePortalAdmins
                && found.Name == "Portal Administrators");
        portalAdminRole.Privs.Should().Contain("security_admin");
        portalAdminRole.Privs.Should().Contain("tenant_read");

        bool hasGlobalPortalAdminLink = await sso.Set<SSOUserRole>()
            .IgnoreQueryFilters()
            .AnyAsync(found => found.UserId == "admin" && found.RoleId == portalAdminRole.Id);
        hasGlobalPortalAdminLink.Should().BeTrue();

        IAccountManager accountManager = harness.Factory.Services.GetRequiredService<IAccountManager>();
        Token loginToken = await accountManager.LoginAsync("admin", "Password123!");
        loginToken.Id.Should().NotBeNullOrWhiteSpace();

        using HttpResponseMessage tenantsResponse =
            await harness.Client.GetAsync($"/Api/Security/Tenant?$format=json&$top=50&$count=true&t={loginToken.Id}");
        string tenantsJson = await tenantsResponse.Content.ReadAsStringAsync();

        tenantsResponse.StatusCode.Should().Be(HttpStatusCode.OK, tenantsJson);
        tenantsJson.Should().Contain("acceptance-platform");

        using HttpResponseMessage userRolesResponse =
            await harness.Client.GetAsync(
                $"/Api/Security/SSOUserRole?$filter=RoleId eq {portalAdminRole.Id}&$expand=User&$format=json&t={loginToken.Id}");
        string userRolesJson = await userRolesResponse.Content.ReadAsStringAsync();

        userRolesResponse.StatusCode.Should().Be(HttpStatusCode.OK, userRolesJson);
        userRolesJson.Should().Contain("admin");
    }

    [Fact]
    public async Task ShouldCreateDatabasesWhenSetupSubmittedAgainstMissingDatabases()
    {
        await using SetupHarness harness = await SetupHarness.CreateAsync();
        await harness.DropDatabasesAsync();

        using HttpResponseMessage setupResponse = await harness.Client.GetAsync("/Setup");
        string setupHtml = await setupResponse.Content.ReadAsStringAsync();

        setupResponse.StatusCode.Should().Be(HttpStatusCode.OK, setupHtml);
        setupHtml.Should().Contain("Welcome to cCoder.Core platform setup");

        await SubmitSetupAsync(harness);

        await using DbContext core = harness.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();
        await using DbContext sso = harness.Factory.Services
            .GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(true);

        (await core.Set<App>().IgnoreQueryFilters().CountAsync()).Should().Be(1);
        (await sso.Set<Tenant>().IgnoreQueryFilters().CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task ShouldRenderAdminPageForAdministratorAndLoginPromptForGuest()
    {
        await using SetupHarness harness = await SetupHarness.CreateAsync();

        await SubmitSetupAsync(harness);

        using HttpClient guestClient = harness.CreateGuestClient();

        await using DbContext core = harness.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        Page adminPage = await core.Set<Page>()
            .IgnoreQueryFilters()
            .SingleAsync(found => found.Path == "Admin/AppManagement");

        Page adminRootPage = await core.Set<Page>()
            .IgnoreQueryFilters()
            .SingleAsync(found => found.Path == "Admin");

        App app = await core.Set<App>()
            .IgnoreQueryFilters()
            .SingleAsync();

        Role administrators = await core.Set<Role>()
            .IgnoreQueryFilters()
            .SingleAsync(found => found.AppId == app.Id && found.Name == "Administrators");

        bool hasPageRole = await core.Set<PageRole>()
            .IgnoreQueryFilters()
            .AnyAsync(found => found.PageId == adminPage.Id && found.RoleId == administrators.Id);

        hasPageRole.Should().BeTrue();

        string[] guestRoleNames = await core.Set<UserRole>()
            .IgnoreQueryFilters()
            .Where(found => found.UserId == "Guest")
            .Join(
                core.Set<Role>().IgnoreQueryFilters(),
                userRole => userRole.RoleId,
                role => role.Id,
                (_, role) => role.Name)
            .OrderBy(name => name)
            .ToArrayAsync();
        guestRoleNames.Should().Equal("Guests");

        string[] adminPageRoleNames = await core.Set<PageRole>()
            .IgnoreQueryFilters()
            .Where(found => found.PageId == adminPage.Id)
            .Join(
                core.Set<Role>().IgnoreQueryFilters(),
                pageRole => pageRole.RoleId,
                role => role.Id,
                (_, role) => role.Name)
            .OrderBy(name => name)
            .ToArrayAsync();

        string[] adminRootPageRoleNames = await core.Set<PageRole>()
            .IgnoreQueryFilters()
            .Where(found => found.PageId == adminRootPage.Id)
            .Join(
                core.Set<Role>().IgnoreQueryFilters(),
                pageRole => pageRole.RoleId,
                role => role.Id,
                (_, role) => role.Name)
            .OrderBy(name => name)
            .ToArrayAsync();

        using HttpResponseMessage adminPagesResponse =
            await harness.Client.GetAsync("/Api/ContentManagement/Page?$filter=Path eq 'Admin/AppManagement'");
        string adminPagesJson = await adminPagesResponse.Content.ReadAsStringAsync();

        adminPagesResponse.StatusCode.Should().Be(HttpStatusCode.OK, adminPagesJson);
        JsonNode adminPagesNode = JsonNode.Parse(adminPagesJson)!;
        JsonArray adminPages = adminPagesNode["value"]?.AsArray() ?? [];
        adminPages.Should().HaveCount(1);
        adminPages[0]?["Path"]?.ToString().Should().Be("Admin/AppManagement");

        using HttpResponseMessage guestPagesResponse =
            await guestClient.GetAsync("/Api/ContentManagement/Page?$filter=Path eq 'Admin/AppManagement'");
        string guestPagesJson = await guestPagesResponse.Content.ReadAsStringAsync();

        guestPagesResponse.StatusCode.Should().Be(HttpStatusCode.OK, guestPagesJson);
        JsonNode guestPagesNode = JsonNode.Parse(guestPagesJson)!;
        JsonArray guestPages = guestPagesNode["value"]?.AsArray() ?? [];
        guestPages.Should().BeEmpty(
            $"guest roles were [{string.Join(", ", guestRoleNames)}], " +
            $"Admin/AppManagement roles were [{string.Join(", ", adminPageRoleNames)}], " +
            $"Admin root roles were [{string.Join(", ", adminRootPageRoleNames)}]");

        using HttpResponseMessage adminResponse = await harness.Client.GetAsync("/Admin/AppManagement");
        string adminHtml = await adminResponse.Content.ReadAsStringAsync();

        adminResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        adminHtml.Should().Contain("App Management");
        adminHtml.Should().Contain("Migrate");
        adminHtml.Should().NotContain("name=\"user\"");
        adminHtml.Should().Contain("Acceptance Admin");

        using HttpResponseMessage guestResponse = await guestClient.GetAsync("/Admin/AppManagement");
        string guestHtml = await guestResponse.Content.ReadAsStringAsync();

        guestResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        guestHtml.Should().Contain("App Management");
        guestHtml.Should().Contain("name=\"user\"");
        guestHtml.Should().Contain("name=\"login\"");
        guestHtml.Should().Contain("name=\"forgotPass\"");
        guestHtml.Should().NotContain("Migrate");
        guestHtml.Should().Contain("<span name=\"userPrefs\" class=\"userPrefs\">Guest</span>");
        guestHtml.Should().Contain("(<a href='/Login'>Login</a>)");
    }

    [Fact]
    public async Task ShouldReturnTopNavRootPagesForAdministratorAndHideAdminMenuForGuest()
    {
        await using SetupHarness harness = await SetupHarness.CreateAsync();

        await SubmitSetupAsync(harness);

        using HttpClient guestClient = harness.CreateGuestClient();

        await using DbContext core = harness.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        App app = await core.Set<App>()
            .IgnoreQueryFilters()
            .SingleAsync();

        string[] guestRoleNames = await core.Set<UserRole>()
            .IgnoreQueryFilters()
            .Where(found => found.UserId == "Guest")
            .Join(
                core.Set<Role>().IgnoreQueryFilters(),
                userRole => userRole.RoleId,
                role => role.Id,
                (_, role) => role.Name)
            .OrderBy(name => name)
            .ToArrayAsync();
        guestRoleNames.Should().Equal("Guests");

        string query =
            $"/Api/ContentManagement/Page?$filter=AppId eq {app.Id} and ParentId eq null and ShowOnMenus eq true&$orderby=Order asc&$expand=PageInfo,Pages($filter=ShowOnMenus eq true;$orderby=Order asc;$expand=PageInfo,Pages($filter=ShowOnMenus eq true;$orderby=Order asc;$expand=PageInfo))";

        using HttpResponseMessage adminResponse = await harness.Client.GetAsync(query);
        string adminJson = await adminResponse.Content.ReadAsStringAsync();

        adminResponse.StatusCode.Should().Be(HttpStatusCode.OK, adminJson);
        JsonNode adminNode = JsonNode.Parse(adminJson)!;
        JsonArray adminPages = adminNode["value"]?.AsArray() ?? [];
        string[] adminPaths = adminPages
            .Select(page => page?["Path"]?.ToString())
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .ToArray()!;

        adminPaths.Should().Contain("Admin");
        adminPaths.Should().Contain("Documentation");
        adminPaths.Should().NotContain("Tools");
        adminPaths.Should().NotContain("Login");
        adminPaths.Should().NotContain("ResetPassword");

        using HttpResponseMessage guestResponse = await guestClient.GetAsync(query);
        string guestJson = await guestResponse.Content.ReadAsStringAsync();

        guestResponse.StatusCode.Should().Be(HttpStatusCode.OK, guestJson);
        JsonNode guestNode = JsonNode.Parse(guestJson)!;
        JsonArray guestPages = guestNode["value"]?.AsArray() ?? [];
        string[] guestPaths = guestPages
            .Select(page => page?["Path"]?.ToString())
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .ToArray()!;

        guestPaths.Should().NotContain("Admin", $"guest roles were [{string.Join(", ", guestRoleNames)}]");
        guestPaths.Should().Contain("Documentation");
        guestPaths.Should().NotContain("Tools");
        guestPaths.Should().NotContain("Login");
        guestPaths.Should().NotContain("ResetPassword");
    }

    private static async Task SubmitSetupAsync(SetupHarness harness)
    {
        HttpClient client = harness.Client;

        using HttpResponseMessage response = await client.PostAsync(
            "/Setup",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("Setup.TenantName", "Acceptance Platform"),
                new KeyValuePair<string, string>("Setup.DisplayName", "Acceptance Admin"),
                new KeyValuePair<string, string>("Setup.Email", "admin@localhost"),
                new KeyValuePair<string, string>("Setup.Password", "Password123!"),
                new KeyValuePair<string, string>("Setup.ConfirmPassword", "Password123!"),
            ]));

        string setupResponseBody = await response.Content.ReadAsStringAsync();

        if (response.StatusCode != HttpStatusCode.Redirect)
        {
            await using DbContext core = harness.Factory.Services
                .GetRequiredService<ICoreContextFactory>()
                .CreateCoreContext();
            await using DbContext sso = harness.Factory.Services
                .GetRequiredService<ISecurityDbContextFactory>()
                .CreateDbContext(true);

            int appCount = await core.Set<App>().IgnoreQueryFilters().CountAsync();
            int userCount = await core.Set<ContentUser>().IgnoreQueryFilters().CountAsync();
            string[] coreUserIds = await core.Set<ContentUser>()
                .IgnoreQueryFilters()
                .OrderBy(found => found.Id)
                .Select(found => found.Id)
                .ToArrayAsync();
            int roleCount = await core.Set<Role>().IgnoreQueryFilters().CountAsync();
            int userRoleCount = await core.Set<UserRole>().IgnoreQueryFilters().CountAsync();
            string[] adminCoreRoles = await core.Set<UserRole>()
                .IgnoreQueryFilters()
                .Where(found => found.UserId == "admin")
                .Join(
                    core.Set<Role>().IgnoreQueryFilters(),
                    userRole => userRole.RoleId,
                    role => role.Id,
                    (_, role) => role.Name)
                .OrderBy(name => name)
                .ToArrayAsync();
            int tenantCount = await sso.Set<Tenant>().IgnoreQueryFilters().CountAsync();
            int ssoUserCount = await sso.Set<SSOUser>().IgnoreQueryFilters().CountAsync();
            int ssoRoleCount = await sso.Set<SSORole>().IgnoreQueryFilters().CountAsync();
            int tokenCount = await sso.Set<Token>().IgnoreQueryFilters().CountAsync();
            string[] tokenReasons = await sso.Set<Token>()
                .IgnoreQueryFilters()
                .OrderBy(found => found.Reason)
                .Select(found => found.Reason.ToString())
                .ToArrayAsync();
            string confirmationTokenId = await sso.Set<Token>()
                .IgnoreQueryFilters()
                .Where(found => found.Reason == 2)
                .Select(found => found.Id)
                .FirstOrDefaultAsync();
            SSOUser adminUser = await sso.Set<SSOUser>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(found => found.Id == "admin");
            Guid? usersRoleId = await core.Set<Role>()
                .IgnoreQueryFilters()
                .Where(found => found.Name == "Users")
                .Select(found => (Guid?)found.Id)
                .FirstOrDefaultAsync();

            string loginProbe;
            try
            {
                IAccountManager accountManager = harness.Factory.Services.GetRequiredService<IAccountManager>();
                var token = await accountManager.LoginAsync("admin", "Password123!");
                loginProbe = $"LoginProbe=OK:{token?.UserName}";
            }
            catch (Exception ex)
            {
                loginProbe = $"LoginProbe={ex.GetType().Name}:{ex.Message}";
            }

            string userVisibilityProbe;
            try
            {
                IUserOrchestrationService userOrchestrationService =
                    harness.Factory.Services.GetRequiredService<IUserOrchestrationService>();
                string[] visibleUserIds = userOrchestrationService.GetAll(true)
                    .OrderBy(found => found.Id)
                    .Select(found => found.Id)
                    .ToArray();
                userVisibilityProbe = $"VisibleUsers=[{string.Join(", ", visibleUserIds)}]";
            }
            catch (Exception ex)
            {
                userVisibilityProbe = $"VisibleUsers={ex.GetType().Name}:{ex.Message}";
            }

            string authorizationProbe;
            try
            {
                IAuthorizationBroker authorizationBroker =
                    harness.Factory.Services.GetRequiredService<IAuthorizationBroker>();
                authorizationProbe = $"CurrentUser={authorizationBroker.GetCurrentUser()?.Id}";
            }
            catch (Exception ex)
            {
                authorizationProbe = $"CurrentUser={ex.GetType().Name}:{ex.Message}";
            }

            string userAddProbe;
            try
            {
                IUserOrchestrationService userOrchestrationService =
                    harness.Factory.Services.GetRequiredService<IUserOrchestrationService>();
                await userOrchestrationService.AddAsync(new cCoder.Data.Models.Security.User
                {
                    Id = "admin",
                    Email = "admin@localhost",
                    DisplayName = "Acceptance Admin",
                    DefaultCultureId = string.Empty,
                    IsActive = true
                });
                userAddProbe = "UserAddProbe=OK";
            }
            catch (Exception ex)
            {
                userAddProbe = $"UserAddProbe={ex.GetType().Name}:{ex.Message}";
            }

            string userRoleProbe;
            try
            {
                IUserRoleOrchestrationService userRoleOrchestrationService =
                    harness.Factory.Services.GetRequiredService<IUserRoleOrchestrationService>();
                await userRoleOrchestrationService.SaveAsync(new UserRole
                {
                    RoleId = usersRoleId ?? Guid.Empty,
                    UserId = "admin"
                });
                userRoleProbe = "UserRoleProbe=OK";
            }
            catch (Exception ex)
            {
                userRoleProbe = $"UserRoleProbe={ex.GetType().Name}:{ex.Message}";
            }

            string confirmProbe;
            try
            {
                if (string.IsNullOrWhiteSpace(confirmationTokenId))
                {
                    confirmProbe = "ConfirmProbe=NoToken";
                }
                else
                {
                    IAccountManager accountManager = harness.Factory.Services.GetRequiredService<IAccountManager>();
                    await accountManager.ConfirmRegistrationAsync(confirmationTokenId);
                    confirmProbe = "ConfirmProbe=OK";
                }
            }
            catch (Exception ex)
            {
                confirmProbe = $"ConfirmProbe={ex.GetType().Name}:{ex.Message}";
            }

            setupResponseBody =
                $"{setupResponseBody}{Environment.NewLine}" +
                $"Core.Apps={appCount}, Core.Users={userCount}, Core.UserIds=[{string.Join(", ", coreUserIds)}], Core.Roles={roleCount}, Core.UserRoles={userRoleCount}, Core.AdminRoles=[{string.Join(", ", adminCoreRoles)}], " +
                $"SSO.Tenants={tenantCount}, SSO.Users={ssoUserCount}, SSO.Roles={ssoRoleCount}, " +
                $"SSO.Admin.EmailConfirmed={adminUser?.EmailConfirmed}, SSO.Admin.Lockout={adminUser?.LockoutEnabled}, " +
                $"SSO.Admin.AccessFailed={adminUser?.AccessFailedCount}, SSO.Admin.HasPassword={!string.IsNullOrWhiteSpace(adminUser?.PasswordHash)}, SSO.Tokens={tokenCount}, SSO.TokenReasons=[{string.Join(", ", tokenReasons)}], " +
                $"{loginProbe}, {authorizationProbe}, {userVisibilityProbe}, {userAddProbe}, {userRoleProbe}, {confirmProbe}";
        }

        response.StatusCode.Should().Be(HttpStatusCode.Redirect, setupResponseBody);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.OriginalString.Should().Be("/");
    }

    private sealed class SetupHarness : IAsyncDisposable
    {
        private readonly AcceptanceDatabaseManager databaseManager;

        private SetupHarness(
            WebAcceptanceFactory factory,
            AcceptanceDatabaseManager databaseManager,
            HttpClient client)
        {
            Factory = factory;
            this.databaseManager = databaseManager;
            Client = client;
        }

        public WebAcceptanceFactory Factory { get; }

        public HttpClient Client { get; }

        public HttpClient CreateGuestClient() =>
            CreateClient(Factory);

        public Task DropDatabasesAsync() =>
            databaseManager.DropDatabasesAsync();

        private static HttpClient CreateClient(WebAcceptanceFactory factory)
        {
            HttpClient client = factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                    BaseAddress = new Uri("https://localhost"),
                });
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("text/html"));
            return client;
        }

        public static async Task<SetupHarness> CreateAsync()
        {
            string suffix = Guid.NewGuid().ToString("N")[..8];
            AcceptanceSettings settings = new()
            {
                CoreConnectionString = AddDatabaseSuffix("CCODER_ACCEPTANCE_CORE_CONNECTION_STRING", suffix),
                SsoConnectionString = AddDatabaseSuffix("CCODER_ACCEPTANCE_SSO_CONNECTION_STRING", suffix),
                DecryptionKey = "000000000000000000000000000000000000000000000000",
            };

            WebAcceptanceFactory factory = new(settings);
            AcceptanceDatabaseManager databaseManager = new(factory.Services);
            await databaseManager.ResetDatabasesAsync();

            HttpClient client = CreateClient(factory);

            return new SetupHarness(factory, databaseManager, client);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await databaseManager.DropDatabasesAsync();
            await Factory.DisposeAsync();
        }

        private static string AddDatabaseSuffix(string variableName, string suffix)
        {
            string connectionString =
                Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(connectionString))
                return string.Empty;

            SqlConnectionStringBuilder builder = new(connectionString)
            {
                Encrypt = true,
                TrustServerCertificate = true,
            };

            string databaseName = builder.InitialCatalog ?? string.Empty;
            if (string.IsNullOrWhiteSpace(databaseName))
                return connectionString;

            builder.InitialCatalog = $"{databaseName}-setup-{suffix}";
            return builder.ConnectionString;
        }
    }
}

