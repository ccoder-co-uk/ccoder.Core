using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions.Execution;
using cCoder.Data;
using cCoder.Data.Models.Packaging;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Security;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Web.AcceptanceTests.Infrastructure;
using Xunit;
using CoreApp = cCoder.Data.Models.CMS.App;


namespace Web.AcceptanceTests.Tests.Api;

[Collection(WebAcceptanceCollection.Name)]
public sealed partial class PackageManagerControllerTests(WebAcceptanceFixture fixture)
{
    private const string AcceptanceAdminRoleName = "Acceptance Administrators";
    private HttpClient Client { get; } = fixture.Client;
    private string BaseUrl { get; } = "/Api/Core/Package";
    private string AppBaseUrl { get; } = "/Api/ContentManagement/App";
    private static JsonSerializerOptions JsonOptions { get; } = new() { PropertyNameCaseInsensitive = true };

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    public static IEnumerable<object[]> CapturedPackageTypeCounts()
    {
        foreach (Package package in AcceptanceSeedData.LoadExportPackages())
        {
            foreach (IGrouping<string, PackageItem> group in (package.Items ?? [])
                .GroupBy(item => item.Type, StringComparer.OrdinalIgnoreCase))
            {
                yield return
                [
                    package.Name,
                    group.Key,
                    group.Sum(item => CountSerializedEntities(item.Data)),
                ];
            }
        }
    }

    private async Task<int> ImportPackageAsync(string body, int appId = 1)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, $"{BaseUrl}/ImportThis?appId={appId}")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };

        using HttpResponseMessage response = await Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        return (int)response.StatusCode;
    }

    private async Task<int> ImportPackageAsync(int appId, Package package)
    {
        using HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{BaseUrl}/Import?appId={appId}",
            package);

        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        return (int)response.StatusCode;
    }

    private async Task<IReadOnlyList<Package>> ExportPackagesAsync(int appId)
    {
        using HttpResponseMessage response = await Client.GetAsync($"{BaseUrl}/Export?appId={appId}");
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        return JsonSerializer.Deserialize<List<Package>>(content, JsonOptions)!;
    }

    private async Task ImportPackagesAsync(int appId, IEnumerable<Package> packages)
    {
        foreach (Package package in packages)
            await ImportPackageAsync(appId, package);
    }

    private async Task<CoreApp> GetStoredAppAsync(int appId)
    {
        await using DbContext core = fixture.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        return await core.Set<CoreApp>()
            .IgnoreQueryFilters()
            .SingleAsync(found => found.Id == appId);
    }

    private async Task<CoreApp> AddAppAsync(CoreApp app)
    {
        await using DbContext core = fixture.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        CoreApp created = (await core.Set<CoreApp>().AddAsync(app)).Entity;
        await core.SaveChangesAsync();
        await GrantGuestAdminAsync(created.Id);
        return created;
    }

    private async Task DeleteStoredAppAsync(int appId)
    {
        await using DbContext core = fixture.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        CoreApp app = await core.Set<CoreApp>()
            .IgnoreQueryFilters()
            .SingleAsync(found => found.Id == appId);

        core.Remove(app);
        await core.SaveChangesAsync();
    }

    private static int CountExportedItems(IReadOnlyList<Package> packages, string packageName, string itemType) =>
        packages
            .Where(package => string.Equals(package.Name, packageName, StringComparison.OrdinalIgnoreCase))
            .SelectMany(package => package.Items ?? [])
            .Count(item => string.Equals(item.Type, itemType, StringComparison.OrdinalIgnoreCase));

    private static int CountExportedEntities(IReadOnlyList<Package> packages, string packageName, string itemType) =>
        packages
            .Where(package => string.Equals(package.Name, packageName, StringComparison.OrdinalIgnoreCase))
            .SelectMany(package => package.Items ?? [])
            .Where(item => string.Equals(item.Type, itemType, StringComparison.OrdinalIgnoreCase))
            .Sum(item => CountSerializedEntities(item.Data));

    private static int CountComparableExportedEntities(IReadOnlyList<Package> packages, string packageName, string itemType) =>
        itemType switch
        {
            "Core/Role" => packages
                .Where(package => string.Equals(package.Name, packageName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(package => package.Items ?? [])
                .Where(item => string.Equals(item.Type, itemType, StringComparison.OrdinalIgnoreCase))
                .Sum(item => CountSerializedObjectsExcluding(item.Data, "Name", AcceptanceAdminRoleName)),
            "Core/PageRole" => packages
                .Where(package => string.Equals(package.Name, packageName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(package => package.Items ?? [])
                .Where(item => string.Equals(item.Type, itemType, StringComparison.OrdinalIgnoreCase))
                .Sum(item => CountSerializedObjectsExcluding(item.Data, "Role", AcceptanceAdminRoleName)),
            _ => CountExportedEntities(packages, packageName, itemType),
        };

    private static int CountSerializedEntities(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
            return 0;

        using JsonDocument document = JsonDocument.Parse(data);

        return document.RootElement.ValueKind switch
        {
            JsonValueKind.Array => document.RootElement.GetArrayLength(),
            JsonValueKind.Object => 1,
            _ => 0,
        };
    }

    private static int CountSerializedObjectsExcluding(string data, string propertyName, string excludedValue)
    {
        if (string.IsNullOrWhiteSpace(data))
            return 0;

        using JsonDocument document = JsonDocument.Parse(data);

        return document.RootElement.ValueKind switch
        {
            JsonValueKind.Array => document.RootElement.EnumerateArray()
                .Count(element => !IsExcluded(element, propertyName, excludedValue)),
            JsonValueKind.Object => IsExcluded(document.RootElement, propertyName, excludedValue) ? 0 : 1,
            _ => 0,
        };
    }

    private static bool IsExcluded(JsonElement element, string propertyName, string excludedValue) =>
        element.TryGetProperty(propertyName, out JsonElement value)
        && string.Equals(value.GetString(), excludedValue, StringComparison.OrdinalIgnoreCase);

    private async Task GrantGuestAdminAsync(int appId)
    {
        await using DbContext core = fixture.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        Role templateRole = await core.Set<Role>()
            .IgnoreQueryFilters()
            .SingleAsync(found => found.AppId == 1 && found.Name == AcceptanceAdminRoleName);

        Role role = await core.Set<Role>()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(found => found.AppId == appId && found.Name == AcceptanceAdminRoleName);

        if (role is null)
        {
            role = new Role
            {
                Id = Guid.NewGuid(),
                AppId = appId,
                Name = templateRole.Name,
                Description = templateRole.Description,
                Privs = templateRole.Privs,
            };

            await core.Set<Role>().AddAsync(role);
        }

        bool hasGuestRole = await core.Set<UserRole>()
            .IgnoreQueryFilters()
            .AnyAsync(found => found.RoleId == role.Id && found.UserId == "Guest");

        if (!hasGuestRole)
            await core.Set<UserRole>().AddAsync(new UserRole { RoleId = role.Id, UserId = "Guest" });

        await core.SaveChangesAsync();
    }
}


