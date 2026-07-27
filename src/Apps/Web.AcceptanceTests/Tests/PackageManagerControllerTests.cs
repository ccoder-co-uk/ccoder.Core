// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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
    private static JsonSerializerOptions JsonOptions { get; } = new() { PropertyNameCaseInsensitive = true };

    private static string Unique(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}";

    public static IEnumerable<object[]> CapturedPackageTypeCounts()
    {
        foreach (Package package in AcceptanceSeedData.LoadExportPackages())
        {
            foreach (IGrouping<string, PackageItem> group in (package.Items ?? [])
                .GroupBy(keySelector: item => item.Type,comparer: StringComparer.OrdinalIgnoreCase))
            {
                yield return
                [
                    package.Name,
                    group.Key,
                    group.Sum(selector: item => CountComparableCapturedEntities(itemType: group.Key,data: item.Data)),
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

        using HttpResponseMessage response = await Client.SendAsync(request: request);
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: content);

        return (int)response.StatusCode;
    }

    private async Task<int> ImportPackageAsync(int appId, Package package)
    {
        using HttpResponseMessage response = await Client.PostAsJsonAsync(
requestUri:             $"{BaseUrl}/Import?appId={appId}",value:             package);

        string content = await response.Content.ReadAsStringAsync();

        if (response.StatusCode != HttpStatusCode.OK)
        {
            content = $"{content}{Environment.NewLine}{fixture.Factory.LogCapture.Read()}";
        }

        response.StatusCode.Should()
            .Be(
                expected: HttpStatusCode.OK,
                because: $"{package.Name}: {content}");

        return (int)response.StatusCode;
    }

    private async Task<IReadOnlyList<Package>> ExportPackagesAsync(int appId)
    {
        using HttpResponseMessage response = await Client.GetAsync(requestUri: $"{BaseUrl}/Export?appId={appId}");
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: content);

        return JsonSerializer.Deserialize<List<Package>>(json: content,options: JsonOptions)!;
    }

    private async Task ImportPackagesAsync(int appId, IEnumerable<Package> packages)
    {
        foreach (Package package in packages)
        {
            await ImportPackageAsync(appId: appId,package: package);

            if (string.Equals(
                a: package.Name,
                b: "Roles",
                comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                await using DbContext core = fixture.Factory.Services
                    .GetRequiredService<ICoreContextFactory>()
                    .CreateCoreContext();

                string[] importedRoleNames = await core.Set<Role>()
                    .IgnoreQueryFilters()
                    .Where(predicate: role => role.AppId == appId)
                    .Select(selector: role => role.Name)
                    .ToArrayAsync();

                importedRoleNames.Should()
                    .Contain(
                        expected: "Administrators",
                        because: $"the Roles package should materialize before dependent packages; found {string.Join(separator: ", ", value: importedRoleNames)}");
            }
        }
    }

    private async Task<CoreApp> GetStoredAppAsync(int appId)
    {
        await using DbContext core = fixture.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        return await core.Set<CoreApp>()
            .IgnoreQueryFilters()
            .SingleAsync(predicate: found => found.Id == appId);
    }

    private async Task<CoreApp> AddAppAsync(CoreApp app)
    {
        await using DbContext core = fixture.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        CoreApp created = (await core.Set<CoreApp>()
            .AddAsync(entity: app)).Entity;

        await core.SaveChangesAsync();
        await GrantGuestAdminAsync(appId: created.Id);
        return created;
    }

    private async Task DeleteStoredAppAsync(int appId)
    {
        await using DbContext core = fixture.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        CoreApp app = await core.Set<CoreApp>()
            .IgnoreQueryFilters()
            .SingleAsync(predicate: found => found.Id == appId);

        core.Remove(entity: app);
        await core.SaveChangesAsync();
    }

    private static int CountExportedItems(IReadOnlyList<Package> packages, string packageName, string itemType) =>
        packages
            .Where(predicate: package => string.Equals(a: package.Name,b: packageName,comparisonType: StringComparison.OrdinalIgnoreCase))
            .SelectMany(selector: package => package.Items ?? [])
            .Count(predicate: item => string.Equals(a: item.Type,b: itemType,comparisonType: StringComparison.OrdinalIgnoreCase));

    private static int CountExportedEntities(IReadOnlyList<Package> packages, string packageName, string itemType) =>
        packages
            .Where(predicate: package => string.Equals(a: package.Name,b: packageName,comparisonType: StringComparison.OrdinalIgnoreCase))
            .SelectMany(selector: package => package.Items ?? [])
            .Where(predicate: item => string.Equals(a: item.Type,b: itemType,comparisonType: StringComparison.OrdinalIgnoreCase))
            .Sum(selector: item => CountSerializedEntities(data: item.Data));

    private static int CountComparableExportedEntities(IReadOnlyList<Package> packages, string packageName, string itemType) =>
        itemType switch
        {
            "Core/Role" => packages
                .Where(predicate: package => string.Equals(a: package.Name,b: packageName,comparisonType: StringComparison.OrdinalIgnoreCase))
                .SelectMany(selector: package => package.Items ?? [])
                .Where(predicate: item => string.Equals(a: item.Type,b: itemType,comparisonType: StringComparison.OrdinalIgnoreCase))
                .Sum(selector: item => CountSerializedObjectsExcluding(data: item.Data,propertyName: "Name",excludedValue: AcceptanceAdminRoleName)),
            "ContentManagement/PageRole" => packages
                .Where(predicate: package => string.Equals(a: package.Name,b: packageName,comparisonType: StringComparison.OrdinalIgnoreCase))
                .SelectMany(selector: package => package.Items ?? [])
                .Where(predicate: item => string.Equals(a: item.Type,b: itemType,comparisonType: StringComparison.OrdinalIgnoreCase))
                .Sum(selector: item => CountSerializedObjectsExcluding(data: item.Data,propertyName: "Role",excludedValue: AcceptanceAdminRoleName)),
            _ => CountExportedEntities(packages: packages,packageName: packageName,itemType: itemType),
        };

    private static int CountComparableCapturedEntities(string itemType, string data) =>
        itemType switch
        {
            "ContentManagement/PageRole" => CountValidPageRoleEntities(data: data),
            _ => CountSerializedEntities(data: data),
        };

    private static int CountSerializedEntities(string data)
    {
        if (string.IsNullOrWhiteSpace(value: data))
        {
            return 0;
        }

        using JsonDocument document = JsonDocument.Parse(json: data);

        return document.RootElement.ValueKind switch
        {
            JsonValueKind.Array => document.RootElement.GetArrayLength(),
            JsonValueKind.Object => 1,
            _ => 0,
        };
    }

    private static int CountSerializedObjectsExcluding(string data, string propertyName, string excludedValue)
    {
        if (string.IsNullOrWhiteSpace(value: data))
        {
            return 0;
        }

        using JsonDocument document = JsonDocument.Parse(json: data);

        return document.RootElement.ValueKind switch
        {
            JsonValueKind.Array => document.RootElement.EnumerateArray()
                .Count(predicate: element => !IsExcluded(element: element,propertyName: propertyName,excludedValue: excludedValue)),
            JsonValueKind.Object => IsExcluded(element: document.RootElement,propertyName: propertyName,excludedValue: excludedValue) ? 0 : 1,
            _ => 0,
        };
    }

    private static int CountValidPageRoleEntities(string data)
    {
        if (string.IsNullOrWhiteSpace(value: data))
        {
            return 0;
        }

        using JsonDocument document = JsonDocument.Parse(json: data);

        return document.RootElement.ValueKind switch
        {
            JsonValueKind.Array => document.RootElement.EnumerateArray()
            .Count(predicate: IsValidPageRole),
            JsonValueKind.Object => IsValidPageRole(element: document.RootElement) ? 1 : 0,
            _ => 0,
        };
    }

    private static bool IsExcluded(JsonElement element, string propertyName, string excludedValue) =>
        element.TryGetProperty(propertyName: propertyName,value: out JsonElement value)
        && string.Equals(a: value.GetString(),b: excludedValue,comparisonType: StringComparison.OrdinalIgnoreCase);

    private static bool IsValidPageRole(JsonElement element) =>
        HasNonEmptyString(element: element,propertyName: "Path")
        && HasNonEmptyString(element: element,propertyName: "Role");

    private static bool HasNonEmptyString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName: propertyName,value: out JsonElement value)
        && !string.IsNullOrWhiteSpace(value: value.GetString());

    private async Task GrantGuestAdminAsync(int appId)
    {
        await using DbContext core = fixture.Factory.Services
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        Role templateRole = await core.Set<Role>()
            .IgnoreQueryFilters()
            .SingleAsync(predicate: found => found.AppId == 1 && found.Name == AcceptanceAdminRoleName);

        Role role = await core.Set<Role>()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(predicate: found => found.AppId == appId && found.Name == AcceptanceAdminRoleName);

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

            await core.Set<Role>()
                .AddAsync(entity: role);
        }

        bool hasGuestRole = await core.Set<UserRole>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: found => found.RoleId == role.Id && found.UserId == "Guest");

        if (!hasGuestRole)
        {
            await core.Set<UserRole>()
                .AddAsync(entity: new UserRole { RoleId = role.Id, UserId = "Guest" });
        }

        await core.SaveChangesAsync();
    }
}