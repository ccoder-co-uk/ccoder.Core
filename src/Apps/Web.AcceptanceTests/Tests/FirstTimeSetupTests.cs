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
using cCoder.Security.Services.Orchestrations.Interfaces;
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

        using HttpResponseMessage setupResponse = await harness.Client.GetAsync("/Setup");

        setupResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        setupResponse.Headers.Location!.OriginalString.Should().Be("/");

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
        string[] folderPaths = await core.Set<Folder>()
            .IgnoreQueryFilters()
            .Select(found => found.Path)
            .OrderBy(path => path)
            .ToArrayAsync();

        folderPaths.Should().Contain(["content", "content/documentation"]);
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
            .Where(found => found.Path == "Clients"
                || found.Path == "Clients/Client")
            .ToDictionaryAsync(found => found.Path, found => found.ShowOnMenus);

        menuVisibility["Clients"].Should().BeTrue();
        menuVisibility["Clients/Client"].Should().BeFalse();

        ContentUser user = await core.Set<ContentUser>()
            .IgnoreQueryFilters()
            .SingleAsync(found => found.Id == "admin");
        user.Email.Should().Be("admin@localhost");

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

        string[] componentCommonObjectNames = commonObjects
            .Where(found => found.Type == "Core/Component")
            .Select(found => found.Name)
            .OrderBy(name => name)
            .ToArray();
        componentCommonObjectNames.Should().Contain(["Client", "ClientList", "TenantManagement"]);

        commonObjects
            .Where(found => found.Type is "Core/Component" or "Core/Resource")
            .Should()
            .OnlyContain(found => !string.IsNullOrWhiteSpace(found.Key));

        commonObjects
            .Where(found => found.Type is "Core/Component" or "Core/Resource")
            .Should()
            .Contain(found => found.Key == "CRM");

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

        IAuthenticationOrchestrationService authenticationService =
            harness.Factory.Services.GetRequiredService<IAuthenticationOrchestrationService>();
        Token loginToken = await authenticationService.LoginAsync("admin", "Password123!");
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
    public async Task ShouldExposeCoreReviewClientPageAfterSetup()
    {
        await using SetupHarness harness = await SetupHarness.CreateAsync();

        await SubmitSetupAsync(harness);

        await using DbContext core = harness.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        Page clientsPage = await core.Set<Page>()
            .IgnoreQueryFilters()
            .SingleAsync(found => found.Path == "Clients");

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

        clientsPage.AppId.Should().Be(app.Id);
        clientsPage.ShowOnMenus.Should().BeTrue();

        using HttpResponseMessage clientsPageResponse =
            await harness.Client.GetAsync("/Api/ContentManagement/Page?$filter=Path eq 'Clients'");
        string clientsPageJson = await clientsPageResponse.Content.ReadAsStringAsync();

        clientsPageResponse.StatusCode.Should().Be(HttpStatusCode.OK, clientsPageJson);
        JsonNode clientsPageNode = JsonNode.Parse(clientsPageJson)!;
        JsonArray clientsPages = clientsPageNode["value"]?.AsArray() ?? [];
        clientsPages.Should().HaveCount(1);
        clientsPages[0]?["Path"]?.ToString().Should().Be("Clients");
    }

    [Fact]
    public async Task ShouldAllowAdministratorToReadGuestUserForRoleManagement()
    {
        await using SetupHarness harness = await SetupHarness.CreateAsync();

        await SubmitSetupAsync(harness);

        using HttpResponseMessage userResponse =
            await harness.Client.GetAsync("/Api/AppSecurity/User?$filter=Id eq 'Guest'");
        string userJson = await userResponse.Content.ReadAsStringAsync();

        userResponse.StatusCode.Should().Be(HttpStatusCode.OK, userJson);
        JsonNode userNode = JsonNode.Parse(userJson)!;
        JsonArray users = userNode["value"]?.AsArray() ?? [];
        users.Should().ContainSingle();
        users[0]?["Id"]?.ToString().Should().Be("Guest");
        users[0]?["DisplayName"]?.ToString().Should().Be("Guest");

        using HttpResponseMessage userRoleResponse =
            await harness.Client.GetAsync("/Api/AppSecurity/UserRole?$filter=UserId eq 'Guest'&$expand=User,Role");
        string userRoleJson = await userRoleResponse.Content.ReadAsStringAsync();

        userRoleResponse.StatusCode.Should().Be(HttpStatusCode.OK, userRoleJson);
        JsonNode userRoleNode = JsonNode.Parse(userRoleJson)!;
        JsonArray userRoles = userRoleNode["value"]?.AsArray() ?? [];
        userRoles.Should().NotBeEmpty();
        userRoles.All(link => link?["User"]?["Id"]?.ToString() == "Guest").Should().BeTrue();
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

        adminPaths.Should().Contain("Clients");
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

        guestPaths.Should().NotContain("Clients");
        guestPaths.Should().NotContain("Admin");
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
                IAuthenticationOrchestrationService authenticationService =
                    harness.Factory.Services.GetRequiredService<IAuthenticationOrchestrationService>();
                var token = await authenticationService.LoginAsync("admin", "Password123!");
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
                    ISSOUserOrchestrationService ssoUserOrchestrationService =
                        harness.Factory.Services.GetRequiredService<ISSOUserOrchestrationService>();
                    await ssoUserOrchestrationService.ConfirmRegistration(confirmationTokenId);
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
            AcceptanceDatabaseManager databaseManager = new(
                factory.Services,
                settings.CoreConnectionString,
                settings.SsoConnectionString);
            await databaseManager.ResetDatabasesAsync();

            HttpClient client = CreateClient(factory);

            return new SetupHarness(factory, databaseManager, client);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();

            try
            {
                await Factory.DisposeAsync();
            }
            finally
            {
                await databaseManager.DropDatabasesAsync();
            }
        }

        private static string AddDatabaseSuffix(string variableName, string suffix)
        {
            string connectionString =
                Environment.GetEnvironmentVariable(variableName)
                ?? Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User)
                ?? Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine)
                ?? string.Empty;

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

