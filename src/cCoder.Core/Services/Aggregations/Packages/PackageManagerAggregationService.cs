// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.Json;
using cCoder.Core.Brokers.Packaging;
using cCoder.Core.Models.Packaging;
using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Packaging;
using cCoder.Data.Models.Security;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Services.Aggregations.Packages;

internal sealed partial class PackageManagerAggregationService(
    IPackageBroker packageBroker,
    ICoreContextFactory coreContextFactory)
    : IPackageManagerAggregationService
{
    private const string AppConfigurationPackageName = "AppConfiguration";
    private const string AppConfigurationItemType = "Core/App";

    private static readonly string[] DefaultPackageNames =
    [
        AppConfigurationPackageName,
        "Roles",
        "Layouts",
        "Templates",
        "Resources",
        "Pages",
        "Workflows",
        "Components",
        "Scripts",
        "PageRoles",
        "FolderRoles",
        "Calendars",
        "CalendarEvents",
    ];

    public ValueTask<Package[]> ExportPackagesAsync(
        int appId,
        string[] packageNames,
        string sourceApi) =>
        TryCatch(operation: async () =>
        {
            ValidatePackagesOnExport(
                appId: appId,
                packageNames: packageNames,
                sourceApi: sourceApi);

            return await ExportPackagesCoreAsync(
                appId: appId,
                packageNames: packageNames,
                sourceApi: sourceApi);
        });

    private async ValueTask<Package[]> ExportPackagesCoreAsync(
        int appId,
        string[] packageNames,
        string sourceApi)
    {
        string[] requestedPackages = packageNames?
            .Where(predicate: packageName =>
                !string.IsNullOrWhiteSpace(value: packageName))
            .ToArray() ?? [];

        if (requestedPackages.Length == 0)
        {
            requestedPackages = DefaultPackageNames;
        }

        List<Package> exportedPackages = [];

        foreach (string packageName in requestedPackages)
        {
            if (string.Equals(
                a: packageName,
                b: AppConfigurationPackageName,
                comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                exportedPackages.Add(
                    item: await ExportAppConfigurationPackageAsync(
                        appId: appId,
                        sourceApi: sourceApi));

                continue;
            }

            if (string.Equals(
                a: packageName,
                b: "PageRoles",
                comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                exportedPackages.Add(
                    item: await ExportPageRolesPackageAsync(
                        appId: appId,
                        sourceApi: sourceApi));

                continue;
            }

            if (string.Equals(
                a: packageName,
                b: "FolderRoles",
                comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                exportedPackages.Add(
                    item: await ExportFolderRolesPackageAsync(
                        appId: appId,
                        sourceApi: sourceApi));

                continue;
            }

            exportedPackages.Add(
                item: packageBroker.ExportPackage(
                    appId: appId,
                    packageName: packageName));
        }

        return [.. exportedPackages];
    }

    private async Task<Package> ExportAppConfigurationPackageAsync(
        int appId,
        string sourceApi)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        App app = await core.Set<App>()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(predicate: found => found.Id == appId)
            ?? throw new InvalidOperationException(
                message: $"App '{appId}' was not found.");

        return new Package
        {
            Name = AppConfigurationPackageName,
            Description = "Application shell configuration",
            Category = "Core",
            SourceApi = sourceApi,
            Items =
            [
                new PackageItem
                {
                    Type = AppConfigurationItemType,
                    Data = JsonSerializer.Serialize(
                        value: new AppConfigurationPackageItem
                        {
                            Id = app.Id,
                            DefaultCultureId = app.DefaultCultureId,
                            TenantId = app.TenantId,
                            Name = app.Name,
                            Domain = app.Domain,
                            DefaultTheme = app.DefaultTheme,
                            ConfigJson = app.ConfigJson,
                        }),
                },
            ],
        };
    }

    private async Task<Package> ExportPageRolesPackageAsync(
        int appId,
        string sourceApi)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        PageRolePackageItem[] rows = await core.Set<PageRole>()
            .IgnoreQueryFilters()
            .Join(
                inner: core.Set<Page>()
                    .IgnoreQueryFilters()
                    .Where(predicate: page => page.AppId == appId),
                outerKeySelector: pageRole => pageRole.PageId,
                innerKeySelector: page => page.Id,
                resultSelector: (pageRole, page) => new { pageRole, page })
            .Join(
                inner: core.Set<Role>()
                    .IgnoreQueryFilters()
                    .Where(predicate: role => role.AppId == appId),
                outerKeySelector: joined => joined.pageRole.RoleId,
                innerKeySelector: role => role.Id,
                resultSelector: (joined, role) => new PageRolePackageItem
                {
                    Path = joined.page.Path,
                    Role = role.Name,
                })
            .ToArrayAsync();

        PageRolePackageItem[] items =
        [
            .. rows.Select(selector: item => new PageRolePackageItem
                {
                    Path = NormalizePagePath(path: item.Path),
                    Role = item.Role,
                })
                .OrderBy(
                    keySelector: item => item.Path,
                    comparer: StringComparer.OrdinalIgnoreCase)
                .ThenBy(
                    keySelector: item => item.Role,
                    comparer: StringComparer.OrdinalIgnoreCase)
        ];

        return new Package
        {
            Name = "PageRoles",
            Description = "Generated by App export.",
            Category = "Dynamic",
            SourceApi = $"{sourceApi}/Api/",
            Items =
            [
                new PackageItem
                {
                    Type = "ContentManagement/PageRole",
                    Data = JsonSerializer.Serialize(value: items),
                },
            ],
        };
    }

    private async Task<Package> ExportFolderRolesPackageAsync(
        int appId,
        string sourceApi)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        FolderRolePackageItem[] rows = await core.Set<FolderRole>()
            .IgnoreQueryFilters()
            .Join(
                inner: core.Set<Folder>()
                    .IgnoreQueryFilters()
                    .Where(predicate: folder => folder.AppId == appId),
                outerKeySelector: folderRole => folderRole.FolderId,
                innerKeySelector: folder => folder.Id,
                resultSelector: (folderRole, folder) => new
                {
                    folderRole,
                    folder,
                })
            .Join(
                inner: core.Set<Role>()
                    .IgnoreQueryFilters()
                    .Where(predicate: role => role.AppId == appId),
                outerKeySelector: joined => joined.folderRole.RoleId,
                innerKeySelector: role => role.Id,
                resultSelector: (joined, role) => new FolderRolePackageItem
                {
                    Path = joined.folder.Path,
                    Name = role.Name,
                })
            .ToArrayAsync();

        FolderRolePackageItem[] items =
        [
            .. rows.Select(selector: item => new FolderRolePackageItem
                {
                    Path = NormalizeFolderPath(path: item.Path),
                    Name = item.Name,
                })
                .OrderBy(
                    keySelector: item => item.Path,
                    comparer: StringComparer.OrdinalIgnoreCase)
                .ThenBy(
                    keySelector: item => item.Name,
                    comparer: StringComparer.OrdinalIgnoreCase)
        ];

        return new Package
        {
            Name = "FolderRoles",
            Description = "Generated by App export.",
            Category = "Dynamic",
            SourceApi = $"{sourceApi}/Api/",
            Items =
            [
                new PackageItem
                {
                    Type = "DocumentManagement/FolderRole",
                    Data = JsonSerializer.Serialize(value: items),
                },
            ],
        };
    }

    private static string NormalizePagePath(string path) =>
        string.IsNullOrWhiteSpace(value: path)
            ? string.Empty
            : path.Trim()
                .Trim(trimChar: '/')
                .Replace(oldChar: '\\', newChar: '/');

    private static string NormalizeFolderPath(string path) =>
        string.IsNullOrWhiteSpace(value: path)
            ? string.Empty
            : path.Trim()
                .Trim(trimChar: '/')
                .Replace(oldChar: '\\', newChar: '/')
                .ToLowerInvariant();
}