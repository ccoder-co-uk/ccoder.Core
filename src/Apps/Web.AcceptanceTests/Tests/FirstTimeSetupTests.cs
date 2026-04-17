using System.Net;
using System.Net.Http.Headers;
using cCoder.Data;
using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
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

public sealed class FirstTimeSetupTests
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
        homeContent.Should().Contain("Welcome to cCoder");
        homeContent.Should().Contain("Open the admin area");
        homeContent.Should().NotContain("Further Information");
        homeContent.Should().NotContain("Corporate LinX");
        homeContent.Should().Contain("/everything.min.js");
        homeContent.Should().Contain("Acceptance Admin");
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

        ContentUser user = await core.Set<ContentUser>()
            .IgnoreQueryFilters()
            .SingleAsync(found => found.Id == "admin");
        user.Email.Should().Be("admin@localhost");

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

        Component topNav = await core.Set<Component>()
            .IgnoreQueryFilters()
            .SingleAsync(found => found.AppId == app.Id && found.Name == "TopNav");
        topNav.Script.Should().Contain("ContentManagement/Page?$filter=AppId eq ");
        topNav.Script.Should().Contain("ParentId eq null");
        topNav.Script.Should().Contain("$expand=PageInfo,Pages(");
        topNav.Script.Should().NotContain("/_navigation/topnav");
        topNav.Script.Should().NotContain("buildTree");
        topNav.Script.Should().NotContain("__allPages");

        Component login = await core.Set<Component>()
            .IgnoreQueryFilters()
            .SingleAsync(found => found.AppId == app.Id && found.Name == "Login");
        login.Script.Should().Contain("$(\"[name=pass]\").val(),");
        login.Script.Should().Contain("session.token = api.token;");
        login.Script.Should().Contain("setUrlQueryParameter(newLocation, \"t\", api.token)");

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
            $"/Api/ContentManagement/Page?$filter=AppId eq {app.Id} and ParentId eq null&$orderby=Order asc&$expand=PageInfo,Pages($orderby=Order asc;$expand=PageInfo,Pages($orderby=Order asc;$expand=PageInfo))";

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
        adminPaths.Should().Contain("Tools");

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
        guestPaths.Should().Contain("Tools");
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

