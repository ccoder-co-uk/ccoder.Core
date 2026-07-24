// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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
using cCoder.Security.Services.Aggregations.Interfaces;
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

        using HttpResponseMessage response = await harness.Client.GetAsync(requestUri: "/");
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK);

        content.Should()
            .Contain(expected: "Welcome to cCoder.Core platform setup");

        content.Should()
            .Contain(expected: "Primary domain:");
    }

    [Fact]
    public async Task ShouldCreateTenantAdminAndBaselineAppWhenSetupSubmitted()
    {
        await using SetupHarness harness = await SetupHarness.CreateAsync();

        await SubmitSetupAsync(harness: harness);

        using HttpResponseMessage setupResponse = await harness.Client.GetAsync(requestUri: "/Setup");

        setupResponse.StatusCode.Should()
            .Be(expected: HttpStatusCode.Redirect);

        setupResponse.Headers.Location!.OriginalString.Should()
            .Be(expected: "/");

        await using DbContext core = harness.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        await using DbContext sso = harness.Factory.Services
            .GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(ignoreAuthInfo: true);

        App app = await core.Set<App>()
            .IgnoreQueryFilters()
            .SingleAsync();

        app.Domain.Should()
            .Be(expected: "localhost");

        app.TenantId.Should()
            .Be(expected: "acceptance-platform");

        (await core.Set<Page>()
            .IgnoreQueryFilters()
            .CountAsync()).Should()
            .BeGreaterThan(expected: 0);

        (await core.Set<Package>()
            .IgnoreQueryFilters()
            .CountAsync()).Should()
            .BeGreaterThan(expected: 0);

        (await core.Set<CommonObject>()
            .IgnoreQueryFilters()
            .CountAsync()).Should()
            .BeGreaterThan(expected: 0);

        string[] folderPaths = await core.Set<Folder>()
            .IgnoreQueryFilters()
            .Select(selector: found => found.Path)
            .OrderBy(keySelector: path => path)
            .ToArrayAsync();

        folderPaths.Should()
            .Contain(expected: ["content", "content/documentation"]);

        folderPaths.Should()
            .NotContain(predicate: path =>
            path.Contains(value: "brandnew270120",comparisonType: StringComparison.OrdinalIgnoreCase)
            || path.Contains(value: "renamed270120",comparisonType: StringComparison.OrdinalIgnoreCase)
            || path.Contains(value: "folderb",comparisonType: StringComparison.OrdinalIgnoreCase)
            || path.Contains(value: "folderc",comparisonType: StringComparison.OrdinalIgnoreCase)
            || path.Contains(value: "testimonial",comparisonType: StringComparison.OrdinalIgnoreCase));

        folderPaths.Should()
            .NotContain(predicate: path =>
            string.Equals(a: path,b: "documentation",comparisonType: StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(value: "documentation/",comparisonType: StringComparison.OrdinalIgnoreCase));

        Dictionary<string, bool> menuVisibility = await core.Set<Page>()
            .IgnoreQueryFilters()
            .Where(predicate: found => found.Path == "Clients"
                || found.Path == "Clients/Client")
            .ToDictionaryAsync(keySelector: found => found.Path,elementSelector: found => found.ShowOnMenus);

        menuVisibility["Clients"].Should()
            .BeTrue();

        menuVisibility["Clients/Client"].Should()
            .BeFalse();

        ContentUser user = await core.Set<ContentUser>()
            .IgnoreQueryFilters()
            .SingleAsync(predicate: found => found.Id == "admin");

        user.Email.Should()
            .Be(expected: "admin@localhost");

        CommonObject[] commonObjects = await core.Set<CommonObject>()
            .IgnoreQueryFilters()
            .ToArrayAsync();

        commonObjects.Should()
            .OnlyContain(predicate: found =>
            found.CreatedBy == user.Id
            && found.LastUpdatedBy == user.Id);

        Role adminRole = await core.Set<Role>()
            .IgnoreQueryFilters()
            .SingleAsync(predicate: found => found.AppId == app.Id && found.Name == "Administrators");

        adminRole.Privs.Should()
            .Contain(expected: "app_create");

        bool hasAdminLink = await core.Set<UserRole>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: found => found.UserId == user.Id && found.RoleId == adminRole.Id);

        hasAdminLink.Should()
            .BeTrue();

        string[] guestRoleNames = await core.Set<UserRole>()
            .IgnoreQueryFilters()
            .Where(predicate: found => found.UserId == "Guest")
            .Join(
inner:                 core.Set<Role>()
            .IgnoreQueryFilters(),outerKeySelector:                 userRole => userRole.RoleId,innerKeySelector:                 role => role.Id,resultSelector:                 (_, role) => role.Name)
            .OrderBy(keySelector: name => name)
            .ToArrayAsync();

        guestRoleNames.Should()
            .Equal(expected: "Guests");

        string[] componentCommonObjectNames = commonObjects
            .Where(predicate: found => found.Type == "Core/Component")
            .Select(selector: found => found.Name)
            .OrderBy(keySelector: name => name)
            .ToArray();

        componentCommonObjectNames.Should()
            .Contain(expected: ["Client", "ClientList", "TenantManagement"]);

        commonObjects
            .Where(predicate: found => found.Type is "Core/Component" or "Core/Resource")
            .Should()
            .OnlyContain(predicate: found => !string.IsNullOrWhiteSpace(value: found.Key));

        commonObjects
            .Where(predicate: found => found.Type is "Core/Component" or "Core/Resource")
            .Should()
            .Contain(predicate: found => found.Key == "CRM");

        Tenant tenant = await sso.Set<Tenant>()
            .IgnoreQueryFilters()
            .SingleAsync(predicate: found => found.Id == "acceptance-platform");

        tenant.Name.Should()
            .Be(expected: "Acceptance Platform");

        SSOUser ssoUser = await sso.Set<SSOUser>()
            .IgnoreQueryFilters()
            .SingleAsync(predicate: found => found.Id == "admin");

        ssoUser.EmailConfirmed.Should()
            .BeTrue();

        bool hasPortalAdminRole = await sso.Set<SSOUserRole>()
            .IgnoreQueryFilters()
            .Include(navigationPropertyPath: found => found.Role)
            .AnyAsync(predicate: found => found.UserId == "admin" && found.Role.Name == "Administrators");

        hasPortalAdminRole.Should()
            .BeTrue();

        SSORole tenantAdminRole = await sso.Set<SSORole>()
            .IgnoreQueryFilters()
            .SingleAsync(predicate: found =>
                found.TenantId == tenant.Id
                && found.UsersArePortalAdmins
                && found.Name == "Administrators");

        tenantAdminRole.Privs.Should()
            .Contain(expected: "tenant_read");

        SSORole portalAdminRole = await sso.Set<SSORole>()
            .IgnoreQueryFilters()
            .SingleAsync(predicate: found =>
                found.TenantId == null
                && found.UsersArePortalAdmins
                && found.Name == "Portal Administrators");

        portalAdminRole.Privs.Should()
            .Contain(expected: "security_admin");

        portalAdminRole.Privs.Should()
            .Contain(expected: "tenant_read");

        bool hasGlobalPortalAdminLink = await sso.Set<SSOUserRole>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: found => found.UserId == "admin" && found.RoleId == portalAdminRole.Id);

        hasGlobalPortalAdminLink.Should()
            .BeTrue();

        IAuthenticationAggregationService authenticationService =
            harness.Factory.Services.GetRequiredService<IAuthenticationAggregationService>();

        Token loginToken = await authenticationService.LoginAsync(username: "admin",password: "Password123!");

        loginToken.Id.Should()
            .NotBeNullOrWhiteSpace();

        using HttpResponseMessage tenantsResponse =
            await harness.Client.GetAsync(requestUri: $"/Api/Security/Tenant?$format=json&$top=50&$count=true&t={loginToken.Id}");

        string tenantsJson = await tenantsResponse.Content.ReadAsStringAsync();

        tenantsResponse.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: tenantsJson);

        tenantsJson.Should()
            .Contain(expected: "acceptance-platform");

        using HttpResponseMessage userRolesResponse =
            await harness.Client.GetAsync(
requestUri:                 $"/Api/Security/SSOUserRole?$filter=RoleId eq {portalAdminRole.Id}&$expand=User&$format=json&t={loginToken.Id}");

        string userRolesJson = await userRolesResponse.Content.ReadAsStringAsync();

        userRolesResponse.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: userRolesJson);

        userRolesJson.Should()
            .Contain(expected: "admin");
    }

    [Fact]
    public async Task ShouldCreateDatabasesWhenSetupSubmittedAgainstMissingDatabases()
    {
        await using SetupHarness harness = await SetupHarness.CreateAsync();
        await harness.DropDatabasesAsync();

        using HttpResponseMessage setupResponse = await harness.Client.GetAsync(requestUri: "/Setup");
        string setupHtml = await setupResponse.Content.ReadAsStringAsync();

        setupResponse.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: setupHtml);

        setupHtml.Should()
            .Contain(expected: "Welcome to cCoder.Core platform setup");

        await SubmitSetupAsync(harness: harness);

        await using DbContext core = harness.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        await using DbContext sso = harness.Factory.Services
            .GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(ignoreAuthInfo: true);

        (await core.Set<App>()
            .IgnoreQueryFilters()
            .CountAsync()).Should()
            .Be(expected: 1);

        (await sso.Set<Tenant>()
            .IgnoreQueryFilters()
            .CountAsync()).Should()
            .Be(expected: 1);
    }

    [Fact]
    public async Task ShouldExposeCoreReviewClientPageAfterSetup()
    {
        await using SetupHarness harness = await SetupHarness.CreateAsync();

        await SubmitSetupAsync(harness: harness);

        await using DbContext core = harness.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        Page clientsPage = await core.Set<Page>()
            .IgnoreQueryFilters()
            .SingleAsync(predicate: found => found.Path == "Clients");

        App app = await core.Set<App>()
            .IgnoreQueryFilters()
            .SingleAsync();

        string[] guestRoleNames = await core.Set<UserRole>()
            .IgnoreQueryFilters()
            .Where(predicate: found => found.UserId == "Guest")
            .Join(
inner:                 core.Set<Role>()
            .IgnoreQueryFilters(),outerKeySelector:                 userRole => userRole.RoleId,innerKeySelector:                 role => role.Id,resultSelector:                 (_, role) => role.Name)
            .OrderBy(keySelector: name => name)
            .ToArrayAsync();

        guestRoleNames.Should()
            .Equal(expected: "Guests");

        clientsPage.AppId.Should()
            .Be(expected: app.Id);

        clientsPage.ShowOnMenus.Should()
            .BeTrue();

        using HttpResponseMessage clientsPageResponse =
            await harness.Client.GetAsync(requestUri: "/Api/ContentManagement/Page?$filter=Path eq 'Clients'");

        string clientsPageJson = await clientsPageResponse.Content.ReadAsStringAsync();

        clientsPageResponse.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: clientsPageJson);

        JsonNode clientsPageNode = JsonNode.Parse(json: clientsPageJson)!;
        JsonArray clientsPages = clientsPageNode["value"]?.AsArray() ?? [];

        clientsPages.Should()
            .HaveCount(expected: 1);

        clientsPages[0]?["Path"]?.ToString()
            .Should()
            .Be(expected: "Clients");
    }

    [Fact]
    public async Task ShouldAllowAdministratorToReadGuestUserForRoleManagement()
    {
        await using SetupHarness harness = await SetupHarness.CreateAsync();

        await SubmitSetupAsync(harness: harness);

        using HttpResponseMessage userResponse =
            await harness.Client.GetAsync(requestUri: "/Api/AppSecurity/User?$filter=Id eq 'Guest'");

        string userJson = await userResponse.Content.ReadAsStringAsync();

        userResponse.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: userJson);

        JsonNode userNode = JsonNode.Parse(json: userJson)!;
        JsonArray users = userNode["value"]?.AsArray() ?? [];

        users.Should()
            .ContainSingle();

        users[0]?["Id"]?.ToString()
            .Should()
            .Be(expected: "Guest");

        users[0]?["DisplayName"]?.ToString()
            .Should()
            .Be(expected: "Guest");

        using HttpResponseMessage userRoleResponse =
            await harness.Client.GetAsync(requestUri: "/Api/AppSecurity/UserRole?$filter=UserId eq 'Guest'&$expand=User,Role");

        string userRoleJson = await userRoleResponse.Content.ReadAsStringAsync();

        userRoleResponse.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: userRoleJson);

        JsonNode userRoleNode = JsonNode.Parse(json: userRoleJson)!;
        JsonArray userRoles = userRoleNode["value"]?.AsArray() ?? [];

        userRoles.Should()
            .NotBeEmpty();

        userRoles.All(predicate: link => link?["User"]?["Id"]?.ToString() == "Guest")
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task ShouldReturnTopNavRootPagesForAdministratorAndHideAdminMenuForGuest()
    {
        await using SetupHarness harness = await SetupHarness.CreateAsync();

        await SubmitSetupAsync(harness: harness);

        using HttpClient guestClient = harness.CreateGuestClient();

        await using DbContext core = harness.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        App app = await core.Set<App>()
            .IgnoreQueryFilters()
            .SingleAsync();

        string[] guestRoleNames = await core.Set<UserRole>()
            .IgnoreQueryFilters()
            .Where(predicate: found => found.UserId == "Guest")
            .Join(
inner:                 core.Set<Role>()
            .IgnoreQueryFilters(),outerKeySelector:                 userRole => userRole.RoleId,innerKeySelector:                 role => role.Id,resultSelector:                 (_, role) => role.Name)
            .OrderBy(keySelector: name => name)
            .ToArrayAsync();

        guestRoleNames.Should()
            .Equal(expected: "Guests");

        string query =
            $"/Api/ContentManagement/Page?$filter=AppId eq {app.Id} and ParentId eq null and ShowOnMenus eq true&$orderby=Order asc&$expand=PageInfo,Pages($filter=ShowOnMenus eq true;$orderby=Order asc;$expand=PageInfo,Pages($filter=ShowOnMenus eq true;$orderby=Order asc;$expand=PageInfo))";

        using HttpResponseMessage adminResponse = await harness.Client.GetAsync(requestUri: query);
        string adminJson = await adminResponse.Content.ReadAsStringAsync();

        adminResponse.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: adminJson);

        JsonNode adminNode = JsonNode.Parse(json: adminJson)!;
        JsonArray adminPages = adminNode["value"]?.AsArray() ?? [];

        string[] adminPaths = adminPages
            .Select(selector: page => page?["Path"]?.ToString())
            .Where(predicate: path => !string.IsNullOrWhiteSpace(value: path))
            .ToArray()!;

        adminPaths.Should()
            .Contain(expected: "Clients");

        adminPaths.Should()
            .Contain(expected: "Admin");

        adminPaths.Should()
            .Contain(expected: "Documentation");

        adminPaths.Should()
            .NotContain(unexpected: "Tools");

        adminPaths.Should()
            .NotContain(unexpected: "Login");

        adminPaths.Should()
            .NotContain(unexpected: "ResetPassword");

        using HttpResponseMessage guestResponse = await guestClient.GetAsync(requestUri: query);
        string guestJson = await guestResponse.Content.ReadAsStringAsync();

        guestResponse.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: guestJson);

        JsonNode guestNode = JsonNode.Parse(json: guestJson)!;
        JsonArray guestPages = guestNode["value"]?.AsArray() ?? [];

        string[] guestPaths = guestPages
            .Select(selector: page => page?["Path"]?.ToString())
            .Where(predicate: path => !string.IsNullOrWhiteSpace(value: path))
            .ToArray()!;

        guestPaths.Should()
            .NotContain(unexpected: "Clients");

        guestPaths.Should()
            .NotContain(unexpected: "Admin");

        guestPaths.Should()
            .NotContain(unexpected: "Tools");

        guestPaths.Should()
            .NotContain(unexpected: "Login");

        guestPaths.Should()
            .NotContain(unexpected: "ResetPassword");
    }

    private static async Task SubmitSetupAsync(SetupHarness harness)
    {
        HttpClient client = harness.Client;

        using HttpResponseMessage response = await client.PostAsync(
requestUri:             "/Setup",content:             new FormUrlEncodedContent(
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
                .CreateDbContext(ignoreAuthInfo: true);

            int appCount = await core.Set<App>()
                .IgnoreQueryFilters()
                .CountAsync();

            int userCount = await core.Set<ContentUser>()
                .IgnoreQueryFilters()
                .CountAsync();

            string[] coreUserIds = await core.Set<ContentUser>()
                .IgnoreQueryFilters()
                .OrderBy(keySelector: found => found.Id)
                .Select(selector: found => found.Id)
                .ToArrayAsync();

            int roleCount = await core.Set<Role>()
                .IgnoreQueryFilters()
                .CountAsync();

            int userRoleCount = await core.Set<UserRole>()
                .IgnoreQueryFilters()
                .CountAsync();

            string[] adminCoreRoles = await core.Set<UserRole>()
                .IgnoreQueryFilters()
                .Where(predicate: found => found.UserId == "admin")
                .Join(
inner:                     core.Set<Role>()
                .IgnoreQueryFilters(),outerKeySelector:                     userRole => userRole.RoleId,innerKeySelector:                     role => role.Id,resultSelector:                     (_, role) => role.Name)
                .OrderBy(keySelector: name => name)
                .ToArrayAsync();

            int tenantCount = await sso.Set<Tenant>()
                .IgnoreQueryFilters()
                .CountAsync();

            int ssoUserCount = await sso.Set<SSOUser>()
                .IgnoreQueryFilters()
                .CountAsync();

            int ssoRoleCount = await sso.Set<SSORole>()
                .IgnoreQueryFilters()
                .CountAsync();

            int tokenCount = await sso.Set<Token>()
                .IgnoreQueryFilters()
                .CountAsync();

            string[] tokenReasons = await sso.Set<Token>()
                .IgnoreQueryFilters()
                .OrderBy(keySelector: found => found.Reason)
                .Select(selector: found => found.Reason.ToString())
                .ToArrayAsync();

            string confirmationTokenId = await sso.Set<Token>()
                .IgnoreQueryFilters()
                .Where(predicate: found => found.Reason == 2)
                .Select(selector: found => found.Id)
                .FirstOrDefaultAsync();

            SSOUser adminUser = await sso.Set<SSOUser>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(predicate: found => found.Id == "admin");

            Guid? usersRoleId = await core.Set<Role>()
                .IgnoreQueryFilters()
                .Where(predicate: found => found.Name == "Users")
                .Select(selector: found => (Guid?)found.Id)
                .FirstOrDefaultAsync();

            string loginProbe;

            try
            {
                IAuthenticationAggregationService authenticationService =
                    harness.Factory.Services.GetRequiredService<IAuthenticationAggregationService>();

                var token = await authenticationService.LoginAsync(username: "admin",password: "Password123!");
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

                string[] visibleUserIds = userOrchestrationService.GetAll(ignoreFilters: true)
                    .OrderBy(keySelector: found => found.Id)
                    .Select(selector: found => found.Id)
                    .ToArray();

                userVisibilityProbe = $"VisibleUsers=[{string.Join(separator: ", ",value: visibleUserIds)}]";
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

                await userOrchestrationService.AddUserAsync(entity: new cCoder.Data.Models.Security.User
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

                await userRoleOrchestrationService.SaveUserRoleAsync(entity: new UserRole
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
                if (string.IsNullOrWhiteSpace(value: confirmationTokenId))
                {
                    confirmProbe = "ConfirmProbe=NoToken";
                }
                else
                {
                    IRegistrationAggregationService registrationAggregationService =
                        harness.Factory.Services.GetRequiredService<IRegistrationAggregationService>();

                    await registrationAggregationService.ConfirmRegistration(tokenId: confirmationTokenId);
                    confirmProbe = "ConfirmProbe=OK";
                }
            }
            catch (Exception ex)
            {
                confirmProbe = $"ConfirmProbe={ex.GetType().Name}:{ex.Message}";
            }

            setupResponseBody =
                $"{setupResponseBody}{Environment.NewLine}" +
                $"Core.Apps={appCount}, Core.Users={userCount}, Core.UserIds=[{string.Join(separator: ", ",value: coreUserIds)}], Core.Roles={roleCount}, Core.UserRoles={userRoleCount}, Core.AdminRoles=[{string.Join(separator: ", ",value: adminCoreRoles)}], " +
                $"SSO.Tenants={tenantCount}, SSO.Users={ssoUserCount}, SSO.Roles={ssoRoleCount}, " +
                $"SSO.Admin.EmailConfirmed={adminUser?.EmailConfirmed}, SSO.Admin.Lockout={adminUser?.LockoutEnabled}, " +
                $"SSO.Admin.AccessFailed={adminUser?.AccessFailedCount}, SSO.Admin.HasPassword={!string.IsNullOrWhiteSpace(value: adminUser?.PasswordHash)}, SSO.Tokens={tokenCount}, SSO.TokenReasons=[{string.Join(separator: ", ",value: tokenReasons)}], " +
                $"{loginProbe}, {authorizationProbe}, {userVisibilityProbe}, {userAddProbe}, {userRoleProbe}, {confirmProbe}";
        }

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.Redirect,because: setupResponseBody);

        response.Headers.Location.Should()
            .NotBeNull();

        response.Headers.Location!.OriginalString.Should()
            .Be(expected: "/");
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
            CreateClient(factory: Factory);

        public Task DropDatabasesAsync() =>
            databaseManager.DropDatabasesAsync();

        private static HttpClient CreateClient(WebAcceptanceFactory factory)
        {
            HttpClient client = factory.CreateClient(
options:                 new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                    BaseAddress = new Uri("https://localhost"),
                });

            client.DefaultRequestHeaders.Accept.Add(
item:                 new MediaTypeWithQualityHeaderValue("text/html"));

            return client;
        }

        public static async Task<SetupHarness> CreateAsync()
        {
            string suffix = Guid.NewGuid()
                .ToString(format: "N")[..8];

            AcceptanceSettings settings = new()
            {
                CoreConnectionString = AddDatabaseSuffix(variableName: "CCODER_ACCEPTANCE_CORE_CONNECTION_STRING",suffix: suffix),
                SsoConnectionString = AddDatabaseSuffix(variableName: "CCODER_ACCEPTANCE_SSO_CONNECTION_STRING",suffix: suffix),
                DecryptionKey = "000000000000000000000000000000000000000000000000",
            };

            WebAcceptanceFactory factory = new(settings);

            AcceptanceDatabaseManager databaseManager = new(
                factory.Services,
                settings.CoreConnectionString,
                settings.SsoConnectionString);

            await databaseManager.ResetDatabasesAsync();

            HttpClient client = CreateClient(factory: factory);

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
                Environment.GetEnvironmentVariable(variable: variableName)
                ?? Environment.GetEnvironmentVariable(variable: variableName,target: EnvironmentVariableTarget.User)
                ?? Environment.GetEnvironmentVariable(variable: variableName,target: EnvironmentVariableTarget.Machine)
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(value: connectionString))
            {
                return string.Empty;
            }

            SqlConnectionStringBuilder builder = new(connectionString)
            {
                Encrypt = true,
                TrustServerCertificate = true,
            };

            string databaseName = builder.InitialCatalog ?? string.Empty;

            if (string.IsNullOrWhiteSpace(value: databaseName))
            {
                return connectionString;
            }

            builder.InitialCatalog = $"{databaseName}-setup-{suffix}";
            return builder.ConnectionString;
        }
    }
}