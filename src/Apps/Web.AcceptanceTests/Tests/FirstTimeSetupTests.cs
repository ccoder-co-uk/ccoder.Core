// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using cCoder.Data;
using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Packaging;
using cCoder.Data.Models.Security;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Data.Models;
using cCoder.Security.Objects.DTOs;
using cCoder.Security.Objects.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
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
        "Packages/First%20Time%20Setup/";

    [Fact]
    public async Task ShouldRenderSetupExperienceWhenEnvironmentIsEmpty()
    {
        // Given
        await using SetupHarness harness = await SetupHarness.CreateAsync();

        // When
        using HttpResponseMessage response =
            await harness.Client.GetAsync(requestUri: "/");

        string content = await response.Content.ReadAsStringAsync();

        // Then
        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK);

        content.Should()
            .Contain(expected: "Welcome to cCoder.Core platform setup");

        content.Should()
            .Contain(expected: "Primary domain:");
    }

    [Fact]
    public async Task ShouldExposeBrowserSetupProgressExperience()
    {
        // Given
        await using SetupHarness harness = await SetupHarness.CreateAsync();

        // When
        using HttpResponseMessage response =
            await harness.Client.GetAsync(requestUri: "/Setup");

        string content = await response.Content.ReadAsStringAsync();

        // Then
        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK);

        content.Should()
            .Contain(expected: "<dialog");

        content.Should()
            .Contain(expected: "setup-log");

        content.Should()
            .Contain(expected: "button.disabled = true");

        content.Should()
            .Contain(expected: "\"/Api/Setup\"");

        content.Should()
            .Contain(expected: "\"/Api/Account/Login\"");

        content.Should()
            .Contain(expected: "\"/Api/ContentManagement/App\"");

        content.Should()
            .Contain(expected: "common-cache");

        content.Should()
            .Contain(expected: "app-baseline");
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
            .Be(expected: "Default");

        Tenant tenant = await sso.Set<Tenant>()
            .IgnoreQueryFilters()
            .SingleAsync();

        tenant.Id.Should()
            .Be(expected: "Default");

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

        pagePaths.Should()
            .NotContain(predicate: path => path.StartsWith(
                value: "Clients",
                comparisonType: StringComparison.OrdinalIgnoreCase));

        (await core.Set<Component>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: component =>
                component.AppId == app.Id
                && component.Name == "Login"
                && component.ResourceKey == "AppSecurity"))
            .Should()
            .BeTrue();

        (await core.Set<Resource>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: resource =>
                resource.AppId == app.Id
                && resource.Key == "AppSecurity"
                && resource.Name == "accessdenied"))
            .Should()
            .BeTrue();

        (await core.Set<Script>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: script =>
                script.AppId == app.Id
                && script.Name == "CoreDailyUsageApiDetailsGrid"))
            .Should()
            .BeTrue();
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
                        Id = "Default",
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

        using HttpResponseMessage appResponse =
            await harness.Client.PostAsJsonAsync(
                requestUri: "/Api/ContentManagement/App",
                value: new
                {
                    name = "Acceptance Platform",
                    domain = "localhost",
                    defaultTheme = "Default",
                    defaultCultureId = string.Empty,
                    tenantId = "Default",
                    configJson = "{}",
                });

        string appContent = await appResponse.Content.ReadAsStringAsync();

        appResponse.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK, because: appContent);

        App app = await appResponse.Content.ReadFromJsonAsync<App>();

        app.Should()
            .NotBeNull();

        using HttpClient assetsClient = new();

        Package[] packages = await Task.WhenAll(
            tasks:
            [
            DownloadPackageAsync(
                client: assetsClient,
                name: "common-cache"),
            DownloadPackageAsync(
                client: assetsClient,
                name: "app-baseline")
            ]);

        foreach (Package package in packages.Reverse())
        {
            await ImportPackageAsync(
                client: harness.Client,
                appId: app!.Id,
                package: package);
        }
    }

    private static async Task<Package> DownloadPackageAsync(
        HttpClient client,
        string name)
    {
        Package package = await client.GetFromJsonAsync<Package>(
            requestUri: $"{AssetsRoot}{name}.json");

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
            requestUri: $"/Api/Core/Package/Import?appId={appId}",
            value: package);

        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(
                expected: HttpStatusCode.OK,
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
            string suffix = Guid.NewGuid()
                .ToString(format: "N")[..8];

            AcceptanceSettings settings = new()
            {
                CoreConnectionString = AddDatabaseSuffix(
                    variableName: "CCODER_ACCEPTANCE_CORE_CONNECTION_STRING",
                    suffix: suffix),
                SsoConnectionString = AddDatabaseSuffix(
                    variableName: "CCODER_ACCEPTANCE_SSO_CONNECTION_STRING",
                    suffix: suffix),
                DecryptionKey =
                    "000000000000000000000000000000000000000000000000",
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

        private static string AddDatabaseSuffix(
            string variableName,
            string suffix)
        {
            string connectionString =
                Environment.GetEnvironmentVariable(variable: variableName)
                ?? Environment.GetEnvironmentVariable(
                    variable: variableName,
                    target: EnvironmentVariableTarget.User)
                ?? Environment.GetEnvironmentVariable(
                    variable: variableName,
                    target: EnvironmentVariableTarget.Machine)
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(value: connectionString))
            {
                return string.Empty;
            }

            SqlConnectionStringBuilder builder = new(
                connectionString: connectionString)
            {
                Encrypt = true,
                TrustServerCertificate = true,
            };

            if (!string.IsNullOrWhiteSpace(value: builder.InitialCatalog))
            {
                builder.InitialCatalog =
                    $"{builder.InitialCatalog}-setup-{suffix}";
            }

            return builder.ConnectionString;
        }
    }
}