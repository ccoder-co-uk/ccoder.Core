// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using cCoder.Data;
using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Packaging;
using cCoder.Data.Models.Security;
using cCoder.Core.Testing;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Data.Models;
using cCoder.Security.Models.DTOs;
using cCoder.Security.Models.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Web.AcceptanceTests.Infrastructure;
using Web.AcceptanceTests.Models;
using Xunit;
using ContentUser = cCoder.Data.Models.Security.User;

namespace Web.AcceptanceTests.Tests;

public sealed partial class FirstTimeSetupTests
{
    private const string AssetsRoot =
        "https://raw.githubusercontent.com/ccoder-co-uk/cCoder.Assets/main/" +
        "Packages/";

    [Fact]
    public async Task ShouldRenderSetupAndBrowserProgressExperienceWhenEnvironmentIsEmpty()
    {
        // Given
        await using SetupHarness harness = await SetupHarness.CreateAsync();

        // When
        using HttpResponseMessage rootResponse =
            await harness.Client.GetAsync(requestUri: "/");

        string rootContent = await rootResponse.Content.ReadAsStringAsync();

        using HttpResponseMessage setupResponse =
            await harness.Client.GetAsync(requestUri: "/Setup");

        string setupContent = await setupResponse.Content.ReadAsStringAsync();

        // Then
        rootResponse.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK);

        rootContent.Should()
            .Contain(expected: "Welcome to cCoder.Core platform setup");

        rootContent.Should()
            .Contain(expected: "FirstAdminUserDetails");

        setupResponse.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK);

        setupContent.Should()
            .Contain(expected: "<dialog");

        setupContent.Should()
            .Contain(expected: "setup-log");

        setupContent.Should()
            .Contain(expected: "button.disabled = true");

        setupContent.Should()
            .Contain(expected: "\"/Api/Setup\"");

        setupContent.Should()
            .Contain(expected: "\"/Api/Account/Login\"");

        setupContent.Should()
            .Contain(expected: "\"/Api/ContentManagement/App\"");

        setupContent.Should()
            .Contain(expected: "common-cache");

        setupContent.Should()
            .Contain(expected: "first-app");

        setupContent.Should()
            .Contain(expected: "\"/Api/Packaging/Package\"");

        setupContent.Should()
            .Contain(expected: "\"/Api/Packaging/PackageItem\"");

        setupContent.Should()
            .Contain(expected: "`/Api/Packaging/Package/Import?appId=${appId}`");

        setupContent.Should()
            .NotContain(unexpected: "/Api/Core/Package/Import");

        setupContent.Should()
            .Contain(expected: "Type: item.Type");

        setupContent.Should()
            .Contain(expected: "\"/Api/RefreshCache\"");
    }

    [Fact]
    public async Task ShouldCompleteFirstTimeSetupWorkflow()
    {
        // Given
        await using SetupHarness harness = await SetupHarness.CreateAsync();

        // When
        await SubmitSetupAsync(harness: harness);

        // Then
        using HttpResponseMessage setupResponse =
            await harness.Client.GetAsync(requestUri: "/Setup");

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
            .Be(expected: "default");

        Tenant tenant = await sso.Set<Tenant>()
            .IgnoreQueryFilters()
            .SingleAsync();

        tenant.Id.Should()
            .Be(expected: "default");

        tenant.Name.Should()
            .Be(expected: "Acceptance Platform");

        SSOUser ssoUser = await sso.Set<SSOUser>()
            .IgnoreQueryFilters()
            .SingleAsync(predicate: user => user.Id == "admin");

        ssoUser.EmailConfirmed.Should()
            .BeTrue();

        ContentUser contentUser = await core.Set<ContentUser>()
            .IgnoreQueryFilters()
            .SingleAsync(predicate: user => user.Id == "admin");

        contentUser.Email.Should()
            .Be(expected: "admin@localhost");

        Role systemAdmins = await core.Set<Role>()
            .IgnoreQueryFilters()
            .SingleAsync(predicate: role =>
                role.AppId == app.Id &&
                role.Name == "System Admins");

        systemAdmins.Privs.Should()
            .Contain(expected: "app_create");

        (await core.Set<UserRole>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: userRole =>
                userRole.UserId == contentUser.Id &&
                userRole.RoleId == systemAdmins.Id))
            .Should()
            .BeTrue();

        string[] pagePaths = await core.Set<Page>()
            .IgnoreQueryFilters()
            .Select(selector: page => page.Path)
            .ToArrayAsync();

        pagePaths.Should()
            .Contain(predicate: path =>
                string.IsNullOrEmpty(value: path));

        pagePaths.Should()
            .Contain(predicate: path => path.StartsWith(
                value: "Admin",
                comparisonType: StringComparison.OrdinalIgnoreCase));

        string[] componentNames = await core.Set<Component>()
            .IgnoreQueryFilters()
            .Select(selector: component =>
                $"{component.AppId}:{component.ResourceKey}/{component.Name}")
            .ToArrayAsync();

        componentNames
            .Should()
            .Contain(
                expected: $"{app.Id}:AppSecurity/Login",
                because: string.Join(separator: ",", value: componentNames));

        (await core.Set<Resource>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: resource =>
                resource.AppId == app.Id
                && resource.Key == "AppSecurity"
                && resource.Name == "accessdenied"))
            .Should()
            .BeTrue();

        (await core.Set<CommonObject>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: commonObject =>
                commonObject.Key == "Security"
                && commonObject.Type == "ContentManagement/Script"
                && commonObject.Name == "CoreDailyUsageApiDetailsGrid"))
            .Should()
            .BeTrue();

        string[] packageNames = await core.Set<Package>()
            .IgnoreQueryFilters()
            .Select(selector: package => package.Name)
            .ToArrayAsync();

        packageNames.Should()
            .Contain(expected: "Baseline New App");

        packageNames.Should()
            .Contain(expected: "DocumentManagement App");

        packageNames.Should()
            .Contain(expected: "DocumentManagement Common Cache");
    }

    private static async Task SubmitSetupAsync(SetupHarness harness)
    {
        using HttpResponseMessage setupResponse =
            await harness.Client.PostAsJsonAsync(
                requestUri: "/Api/Setup",
                value: new SetupDetails
                {
                    Tenant = new Tenant
                    {
                        Id = "default",
                        Name = "Acceptance Platform",
                    },
                    User = new SSOUser
                    {
                        DisplayName = "Acceptance Admin",
                        Email = "admin@localhost",
                        PasswordHash = "Password123!",
                        PhoneNumber = string.Empty,
                    },
                });

        string setupContent = await setupResponse.Content.ReadAsStringAsync();

        setupResponse.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK, because: setupContent);

        using HttpResponseMessage loginResponse =
            await harness.Client.PostAsJsonAsync(
                requestUri: "/Api/Account/Login",
                value: new Auth
                {
                    User = "admin",
                    Pass = "Password123!",
                });

        string loginContent = await loginResponse.Content.ReadAsStringAsync();

        loginResponse.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK, because: loginContent);

        Token token = await loginResponse.Content.ReadFromJsonAsync<Token>();

        token.Should()
            .NotBeNull();

        harness.Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                scheme: "Bearer",
                parameter: token!.Id);

        await using DbContext core = harness.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        string[] userIds = await core.Set<ContentUser>()
            .IgnoreQueryFilters()
            .Select(selector: user => user.Id)
            .ToArrayAsync();

        userIds.Should()
            .Contain(
                expected: token.UserName,
                because: string.Join(separator: ",", value: userIds));

        using HttpResponseMessage appResponse = await harness.Client.PostAsJsonAsync(
            requestUri: "/Api/ContentManagement/App",
            value: new
            {
                Name = "First Time Setup",
                Domain = "localhost",
                TenantId = "default",
            });

        string appContent = await appResponse.Content.ReadAsStringAsync();

        if (appResponse.StatusCode != HttpStatusCode.Created)
        {
            string[] appState = await core.Set<App>()
                .IgnoreQueryFilters()
                .Select(selector: app =>
                    $"{app.Id}:{app.Name}:{app.Domain}:{app.TenantId}")
                .ToArrayAsync();

            appContent = $"{appContent}{Environment.NewLine}Apps: {string.Join(separator: ", ", value: appState)}";
        }

        appResponse.StatusCode.Should()
            .Be(expected: HttpStatusCode.Created, because: appContent);

        App app = await appResponse.Content.ReadFromJsonAsync<App>();

        app.Should()
            .NotBeNull();

        using HttpClient assetsClient = new();

        Package[] packages = await Task.WhenAll(
            tasks:
            [
            DownloadPackageAsync(
                client: assetsClient,
                path: "First%20Time%20Setup/common-cache.json"),
            DownloadPackageAsync(
                client: assetsClient,
                path: "First%20Time%20Setup/first-app.json")
            ]);

        await ImportCommonCachePackageAsync(
            client: harness.Client,
            package: packages[0]);

        await ImportPackageAsync(
            client: harness.Client,
            appId: app!.Id,
            package: packages[1]);

        await WaitForBaselineImportsAsync(
            harness: harness,
            appId: app.Id);

        string[] packagingRoutes = harness.Factory.Services
            .GetServices<Microsoft.AspNetCore.Routing.EndpointDataSource>()
            .SelectMany(selector: source => source.Endpoints)
            .OfType<Microsoft.AspNetCore.Routing.RouteEndpoint>()
            .Where(predicate: endpoint =>
                endpoint.RoutePattern.RawText?.Contains(
                    value: "Packaging/Package",
                    comparisonType: StringComparison.OrdinalIgnoreCase) == true)
            .Select(selector: endpoint =>
                $"{endpoint.RoutePattern.RawText}: {string.Join(
                    separator: ",",
                    values: endpoint.Metadata
                        .GetMetadata<Microsoft.AspNetCore.Routing.HttpMethodMetadata>()
                        ?.HttpMethods
                    ?? [])}")
            .ToArray();

        packagingRoutes.Should()
            .Contain(
                predicate: route => route.EndsWith(
                    value: ": POST",
                    comparisonType: StringComparison.Ordinal),
                because: string.Join(
                    separator: "; ",
                    value: packagingRoutes));

        await RegisterReusablePackagesAsync(
            assetsClient: assetsClient,
            apiClient: harness.Client);
    }

    private static async Task WaitForBaselineImportsAsync(
        SetupHarness harness,
        int appId)
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            await using DbContext core = harness.Factory.Services
                .GetRequiredService<ICoreContextFactory>()
                .CreateCoreContext();

            bool commonCacheReady = await core.Set<CommonObject>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: commonObject =>
                    commonObject.Key == "Security"
                    && commonObject.Type == "ContentManagement/Script"
                    && commonObject.Name == "CoreDailyUsageApiDetailsGrid");

            bool appReady = await core.Set<Component>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: component =>
                    component.AppId == appId
                    && component.ResourceKey == "AppSecurity"
                    && component.Name == "Login");

            if (commonCacheReady && appReady)
            {
                return;
            }

            await Task.Delay(millisecondsDelay: 100);
        }

        throw new TimeoutException(
            message: "The first-time setup baseline packages did not finish importing.");
    }

    private static async Task RegisterReusablePackagesAsync(
        HttpClient assetsClient,
        HttpClient apiClient)
    {
        JsonObject manifest = await assetsClient
            .GetFromJsonAsync<JsonObject>(
                requestUri: $"{AssetsRoot}manifest.json");

        manifest.Should()
            .NotBeNull();

        foreach (JsonNode packageNode in manifest!["Packages"]!.AsArray())
        {
            JsonObject manifestPackage = packageNode.AsObject();

            if (manifestPackage["FirstTimeSetup"]!.GetValue<bool>())
            {
                continue;
            }

            Package package = await DownloadPackageAsync(
                client: assetsClient,
                path: Uri.EscapeDataString(
                        stringToEscape: manifestPackage["Path"]!
                            .GetValue<string>())
                    .Replace(
                        oldValue: "%2F",
                        newValue: "/",
                        comparisonType: StringComparison.OrdinalIgnoreCase));

            using HttpResponseMessage response =
                await apiClient.PostAsJsonAsync(
                    requestUri: "/Api/Packaging/Package",
                    value: package);

            string content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should()
                .Be(
                    expected: HttpStatusCode.Created,
                    because: $"{content}; {response.Headers}");
        }
    }

    private static async Task ImportCommonCachePackageAsync(
        HttpClient client,
        Package package)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            requestUri: "/Api/Packaging/Package/Import",
            value: package);

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.Accepted);
    }

    private static async Task<Package> DownloadPackageAsync(
        HttpClient client,
        string path)
    {
        Package package = await client.GetFromJsonAsync<Package>(
            requestUri: $"{AssetsRoot}{path}");

        package.Should()
            .NotBeNull();

        return package!;
    }

    private static async Task ImportPackageAsync(
        HttpClient client,
        int appId,
        Package package)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            requestUri: $"/Api/Packaging/Package/Import?appId={appId}",
            value: package);

        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(
                expected: HttpStatusCode.Accepted,
                because: $"{package.Name}: {content}");

        content.Should()
            .NotContain(
                unexpected: "\"Success\":false",
                because: $"{package.Name}: {content}");
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

        public static async Task<SetupHarness> CreateAsync()
        {
            AcceptanceTestConfiguration configuration =
                AcceptanceTestConfiguration.Load();

            AcceptanceSettings settings = new()
            {
                CoreConnectionString = configuration.CoreConnectionString,
                SsoConnectionString =
                    configuration.SecurityConnectionString,
                DecryptionKey = configuration.DecryptionKey,
            };

            WebAcceptanceFactory factory = new(settings: settings);

            AcceptanceDatabaseManager databaseManager = new(
                factory.Services,
                settings.CoreConnectionString,
                settings.SsoConnectionString);

            await databaseManager.ResetDatabasesAsync();

            HttpClient client = factory.CreateClient(
                options: new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                    BaseAddress = new Uri(uriString: "https://localhost"),
                });

            client.Timeout = TimeSpan.FromMinutes(value: 10);

            client.DefaultRequestHeaders.Accept.Add(
                item: new MediaTypeWithQualityHeaderValue(
                    mediaType: "application/json"));

            return new SetupHarness(
                factory: factory,
                databaseManager: databaseManager,
                client: client);
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
    }
}