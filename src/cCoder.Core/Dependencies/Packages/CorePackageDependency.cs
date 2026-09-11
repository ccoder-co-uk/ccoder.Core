// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.Json;
using System.Text.Json.Nodes;
using cCoder.Core.Models.Packaging;
using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Packaging;
using cCoder.Data.Models.Security;
using Microsoft.EntityFrameworkCore;
using cCoder.Packaging.Exposures;

namespace cCoder.Core.Dependencies.Packages;

internal sealed class CorePackageDependency(
    ICoreContextFactory coreContextFactory,
    IPackageTransferManager packageTransferManager)
{
    private const string AppConfigurationPackageName = "AppConfiguration";
    private const string AppConfigurationItemType = "Core/App";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async ValueTask ImportAppConfigurationAsync(
        int appId,
        string data)
    {
        AppConfigurationPackageItem imported = DeserializeAppConfiguration(data: data);

        if (imported is null)
        {
            return;
        }

        await using DbContext core = coreContextFactory.CreateCoreContext();

        App app = await core.Set<App>()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(predicate: found => found.Id == appId)
            ?? throw new InvalidOperationException(message: $"App '{appId}' was not found.");

        app.DefaultCultureId = imported.DefaultCultureId ?? string.Empty;
        app.Name = imported.Name ?? app.Name;
        app.DefaultTheme = imported.DefaultTheme ?? app.DefaultTheme;
        app.ConfigJson = imported.ConfigJson ?? app.ConfigJson;

        await core.SaveChangesAsync();
    }

    public async ValueTask<Package> ExportAppConfigurationAsync(
        int appId,
        string sourceApi)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        App app = await core.Set<App>()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(predicate: found => found.Id == appId)
            ?? throw new InvalidOperationException(message: $"App '{appId}' was not found.");

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
                    Data = JsonSerializer.Serialize(value: new AppConfigurationPackageItem
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

    public async ValueTask<Package> ExportPageRolesAsync(
        int appId,
        string sourceApi)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        PageRolePackageItem[] rows = await core.Set<PageRole>()
            .IgnoreQueryFilters()
            .Join(
                inner: core.Set<Page>().IgnoreQueryFilters().Where(predicate: page => page.AppId == appId),
                outerKeySelector: pageRole => pageRole.PageId,
                innerKeySelector: page => page.Id,
                resultSelector: (pageRole, page) => new { pageRole, page })
            .Join(
                inner: core.Set<Role>().IgnoreQueryFilters().Where(predicate: role => role.AppId == appId),
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
                .OrderBy(keySelector: item => item.Path, comparer: StringComparer.OrdinalIgnoreCase)
                .ThenBy(keySelector: item => item.Role, comparer: StringComparer.OrdinalIgnoreCase)
        ];

        return CreatePackage(
            name: "PageRoles",
            sourceApi: sourceApi,
            itemType: "ContentManagement/PageRole",
            data: JsonSerializer.Serialize(value: items));
    }

    public async ValueTask<Package> ExportFolderRolesAsync(
        int appId,
        string sourceApi)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        FolderRolePackageItem[] rows = await core.Set<FolderRole>()
            .IgnoreQueryFilters()
            .Join(
                inner: core.Set<Folder>().IgnoreQueryFilters().Where(predicate: folder => folder.AppId == appId),
                outerKeySelector: folderRole => folderRole.FolderId,
                innerKeySelector: folder => folder.Id,
                resultSelector: (folderRole, folder) => new { folderRole, folder })
            .Join(
                inner: core.Set<Role>().IgnoreQueryFilters().Where(predicate: role => role.AppId == appId),
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
                .OrderBy(keySelector: item => item.Path, comparer: StringComparer.OrdinalIgnoreCase)
                .ThenBy(keySelector: item => item.Name, comparer: StringComparer.OrdinalIgnoreCase)
        ];

        return CreatePackage(
            name: "FolderRoles",
            sourceApi: sourceApi,
            itemType: "DocumentManagement/FolderRole",
            data: JsonSerializer.Serialize(value: items));
    }

    public Package ExportPackage(
        int appId,
        string packageName) =>
        packageTransferManager.ExportPackage(
            appId: appId,
            packageName: packageName);

    private static Package CreatePackage(string name, string sourceApi, string itemType, string data) =>
        new()
        {
            Name = name,
            Description = "Generated by App export.",
            Category = "Dynamic",
            SourceApi = $"{sourceApi}/Api/",
            Items = [new PackageItem { Type = itemType, Data = data }],
        };

    private static AppConfigurationPackageItem DeserializeAppConfiguration(string data)
    {
        if (string.IsNullOrWhiteSpace(value: data))
        {
            return null;
        }

        JsonNode node = JsonNode.Parse(json: data);

        if (node is null)
        {
            return null;
        }

        RemoveTypeMetadata(node: node);

        return node switch
        {
            JsonArray array => array.Deserialize<AppConfigurationPackageItem[]>(options: JsonOptions)?.FirstOrDefault(),
            JsonObject jsonObject => jsonObject.Deserialize<AppConfigurationPackageItem>(options: JsonOptions),
            _ => null,
        };
    }

    private static void RemoveTypeMetadata(JsonNode node)
    {
        switch (node)
        {
            case JsonObject jsonObject:
                jsonObject.Remove(propertyName: "$type");

                foreach (JsonNode child in jsonObject.Select(selector: property => property.Value)
                    .Where(predicate: value => value is not null))
                {
                    RemoveTypeMetadata(node: child);
                }

                break;

            case JsonArray jsonArray:
                foreach (JsonNode child in jsonArray.Where(predicate: value => value is not null))
                {
                    RemoveTypeMetadata(node: child);
                }

                break;
        }
    }

    private static string NormalizePagePath(string path) =>
        string.IsNullOrWhiteSpace(value: path)
            ? string.Empty
            : path.Trim().Trim(trimChar: '/').Replace(oldChar: '\\', newChar: '/');

    private static string NormalizeFolderPath(string path) =>
        string.IsNullOrWhiteSpace(value: path)
            ? string.Empty
            : path.Trim().Trim(trimChar: '/').Replace(oldChar: '\\', newChar: '/').ToLowerInvariant();
}