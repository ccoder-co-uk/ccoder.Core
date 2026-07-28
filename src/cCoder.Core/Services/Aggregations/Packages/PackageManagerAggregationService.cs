// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.Json;
using System.Text.Json.Nodes;
using cCoder.ContentManagement.Services.Orchestrations;
using cCoder.Core.Models.Packaging;
using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Security;
using cCoder.Packaging.Models;
using cCoder.Data.Models.Packaging;
using cCoder.Core.Brokers.Packaging;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Services.Aggregations.Packages;

internal sealed partial class PackageManagerAggregationService(
    IPackageBroker packageBroker,
    IComponentOrchestrationService componentOrchestrationService,
    ILayoutOrchestrationService layoutOrchestrationService,
    IResourceOrchestrationService resourceOrchestrationService,
    IScriptOrchestrationService scriptOrchestrationService,
    ITemplateOrchestrationService templateOrchestrationService,
    ICoreContextFactory coreContextFactory
) : IPackageManagerAggregationService
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

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

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
        string[] requestedPackages =
            packageNames?.Where(predicate: packageName => !string.IsNullOrWhiteSpace(value: packageName))
                .ToArray()
            ?? [];

        if (requestedPackages.Length == 0)
        {
            requestedPackages = DefaultPackageNames;
        }

        List<Package> exportedPackages = [];

        foreach (string packageName in requestedPackages)
        {
            if (string.Equals(a: packageName, b: AppConfigurationPackageName, comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                exportedPackages.Add(
                    item: await ExportAppConfigurationPackageAsync(
                        appId: appId,
                        sourceApi: sourceApi));

                continue;
            }

            if (string.Equals(a: packageName, b: "PageRoles", comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                exportedPackages.Add(
                    item: await ExportPageRolesPackageAsync(
                        appId: appId,
                        sourceApi: sourceApi));

                continue;
            }

            if (string.Equals(a: packageName, b: "FolderRoles", comparisonType: StringComparison.OrdinalIgnoreCase))
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

    public ValueTask ImportPackagesAsync(
        int appId,
        IEnumerable<Package> packages) =>
        TryCatch(operation: async () =>
        {
            ValidatePackagesOnImport(
                appId: appId,
                packages: packages);

            await ImportPackagesCoreAsync(
                appId: appId,
                packages: packages);
        });

    private async ValueTask ImportPackagesCoreAsync(
        int appId,
        IEnumerable<Package> packages)
    {
        foreach (Package package in packages ?? [])
        {
            Package sanitizedPackage = SanitizePackage(package: package);

            PackageItem[] appItems = [.. (sanitizedPackage.Items ?? []).Where(predicate: item => string.Equals(a: item.Type, b: AppConfigurationItemType, comparisonType: StringComparison.OrdinalIgnoreCase))];

            if (appItems.Length > 0)
            {
                foreach (PackageItem appItem in appItems)
                {
                    await ImportAppConfigurationAsync(appId: appId, packageItem: appItem);
                }
            }

            PackageItem[] contentManagementItems = [.. (sanitizedPackage.Items ?? [])
                .Where(predicate: item =>
                    item.Type.StartsWith(
                        value: "ContentManagement/",
                        comparisonType: StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(a: item.Type, b: AppConfigurationItemType, comparisonType: StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(a: item.Type, b: "ContentManagement/Page", comparisonType: StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(a: item.Type, b: "ContentManagement/PageRole", comparisonType: StringComparison.OrdinalIgnoreCase))];

            if (contentManagementItems.Length > 0)
            {
                await ImportContentManagementItemsAsync(
                    appId: appId,
                    packageItems: contentManagementItems);
            }

            PackageItem[] remainingItems = [.. (sanitizedPackage.Items ?? [])
                .Where(predicate: item =>
                    !item.Type.StartsWith(
                        value: "ContentManagement/",
                        comparisonType: StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(a: item.Type, b: "DocumentManagement/FolderRole", comparisonType: StringComparison.OrdinalIgnoreCase))];

            if (remainingItems.Length > 0)
            {
                Package remainingPackage = new(sanitizedPackage.Name)
                {
                    Id = sanitizedPackage.Id,
                    Description = sanitizedPackage.Description,
                    Category = sanitizedPackage.Category,
                    SourceApi = sanitizedPackage.SourceApi,
                    Items = remainingItems,
                };

                await packageBroker.ImportPackageAsync(
                    appId: appId,
                    package: remainingPackage);
            }

            PackageItem[] pageItems = [.. (sanitizedPackage.Items ?? []).Where(predicate: item => string.Equals(a: item.Type, b: "ContentManagement/Page", comparisonType: StringComparison.OrdinalIgnoreCase))];

            if (pageItems.Length > 0)
            {
                await ImportPagesAsync(appId: appId, pageItems: pageItems);
            }

            PackageItem[] pageRoleItems = [.. (sanitizedPackage.Items ?? []).Where(predicate: item => string.Equals(a: item.Type, b: "ContentManagement/PageRole", comparisonType: StringComparison.OrdinalIgnoreCase))];

            if (pageRoleItems.Length > 0)
            {
                await ImportPageRolesAsync(appId: appId, pageRoleItems: pageRoleItems);
            }

            PackageItem[] folderRoleItems = [.. (sanitizedPackage.Items ?? []).Where(predicate: item => string.Equals(a: item.Type, b: "DocumentManagement/FolderRole", comparisonType: StringComparison.OrdinalIgnoreCase))];

            if (folderRoleItems.Length > 0)
            {
                await ImportFolderRolesAsync(appId: appId, folderRoleItems: folderRoleItems);
            }
        }
    }

    private async Task ImportContentManagementItemsAsync(
        int appId,
        IEnumerable<PackageItem> packageItems)
    {
        foreach (PackageItem packageItem in packageItems)
        {
            switch (packageItem.Type)
            {
                case "ContentManagement/Component":
                    await componentOrchestrationService.ImportComponentsAsync(
                        appId: appId,
                        items: DeserializePackageItems<Component>(
                            data: packageItem.Data));
                    break;

                case "ContentManagement/Layout":
                    await layoutOrchestrationService.ImportLayoutsAsync(
                        appId: appId,
                        items: DeserializePackageItems<Layout>(
                            data: packageItem.Data));
                    break;

                case "ContentManagement/Resource":
                    await resourceOrchestrationService.ImportResourcesAsync(
                        appId: appId,
                        items: DeserializePackageItems<Resource>(
                            data: packageItem.Data));
                    break;

                case "ContentManagement/Script":
                    await scriptOrchestrationService.ImportScriptsAsync(
                        appId: appId,
                        items: DeserializePackageItems<Script>(
                            data: packageItem.Data));
                    break;

                case "ContentManagement/Template":
                    await templateOrchestrationService.ImportTemplatesAsync(
                        appId: appId,
                        items: DeserializePackageItems<Template>(
                            data: packageItem.Data));
                    break;
            }
        }
    }

    private async Task ImportPagesAsync(int appId, IEnumerable<PackageItem> pageItems)
    {
        Page[] items = [.. pageItems
            .SelectMany(selector: item => DeserializePackageItems<Page>(data: item.Data))
            .OrderBy(keySelector: item => GetPageDepth(path: item.Path))
            .ThenBy(keySelector: item => item.Order)];

        if (items.Length == 0)
        {
            return;
        }

        await using DbContext core = coreContextFactory.CreateCoreContext();

        foreach (Page item in items)
        {
            string normalizedPath = NormalizePagePath(path: item.Path);
            string parentPath = GetParentPagePath(path: normalizedPath);

            Page parent = string.IsNullOrWhiteSpace(value: parentPath)
                ? null
                : await core.Set<Page>()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(predicate: found => found.AppId == appId && found.Path == parentPath);

            int? parentId = parent?.Id;

            Page existingPage = await core.Set<Page>()
                .IgnoreQueryFilters()
                .Include(navigationPropertyPath: found => found.PageInfo)
                .Include(navigationPropertyPath: found => found.Contents)
                .FirstOrDefaultAsync(predicate: found => found.AppId == appId && found.Path == normalizedPath);

            existingPage ??= await core.Set<Page>()
                .IgnoreQueryFilters()
                .Include(navigationPropertyPath: found => found.PageInfo)
                .Include(navigationPropertyPath: found => found.Contents)
                .FirstOrDefaultAsync(predicate: found =>
                    found.AppId == appId
                    && found.Name == item.Name
                    && found.ParentId == parentId);

            if (existingPage is null)
            {
                await core.Set<Page>()
                    .AddAsync(entity: new Page
                    {
                        AppId = appId,
                        ParentId = parentId,
                        Order = item.Order,
                        ShowOnMenus = item.ShowOnMenus,
                        Name = item.Name,
                        LastUpdated = item.LastUpdated,
                        LastUpdatedBy = item.LastUpdatedBy,
                        CreatedOn = item.CreatedOn,
                        CreatedBy = item.CreatedBy,
                        Path = normalizedPath,
                        ResourceKey = item.ResourceKey,
                        Layout = item.Layout,
                        PageInfo = [.. (item.PageInfo ?? [])
                        .Select(selector: info => new PageInfo
                        {
                            CultureId = info.CultureId,
                            Title = info.Title,
                            Description = info.Description,
                            Keywords = info.Keywords,
                        })],
                        Contents = [.. (item.Contents ?? [])
                        .Select(selector: content => new cCoder.Data.Models.CMS.Content
                        {
                            CultureId = content.CultureId,
                            Name = content.Name,
                            Html = content.Html,
                        })],
                    });

                await core.SaveChangesAsync();
                continue;
            }

            existingPage.ParentId = parentId;
            existingPage.Order = item.Order;
            existingPage.ShowOnMenus = item.ShowOnMenus;
            existingPage.Name = item.Name;
            existingPage.LastUpdated = item.LastUpdated;
            existingPage.LastUpdatedBy = item.LastUpdatedBy;
            existingPage.CreatedOn = item.CreatedOn;
            existingPage.CreatedBy = item.CreatedBy;
            existingPage.Path = normalizedPath;
            existingPage.ResourceKey = item.ResourceKey;
            existingPage.Layout = item.Layout;

            core.Set<PageInfo>()
                .RemoveRange(entities: existingPage.PageInfo ?? []);

            core.Set<cCoder.Data.Models.CMS.Content>()
                .RemoveRange(entities: existingPage.Contents ?? []);

            existingPage.PageInfo = [.. (item.PageInfo ?? [])
                .Select(selector: info => new PageInfo
                {
                    PageId = existingPage.Id,
                    CultureId = info.CultureId,
                    Title = info.Title,
                    Description = info.Description,
                    Keywords = info.Keywords,
                })];

            existingPage.Contents = [.. (item.Contents ?? [])
                .Select(selector: content => new cCoder.Data.Models.CMS.Content
                {
                    PageId = existingPage.Id,
                    CultureId = content.CultureId,
                    Name = content.Name,
                    Html = content.Html,
                })];

            await core.SaveChangesAsync();
        }
    }

    private async Task ImportPageRolesAsync(int appId, IEnumerable<PackageItem> pageRoleItems)
    {
        PageRolePackageItem[] items = [.. pageRoleItems
            .SelectMany(selector: item => DeserializePackageItems<PageRolePackageItem>(data: item.Data))
            .Where(predicate: item => !string.IsNullOrWhiteSpace(value: item.Path) && !string.IsNullOrWhiteSpace(value: item.Role))];

        if (items.Length == 0)
        {
            return;
        }

        await using DbContext core = coreContextFactory.CreateCoreContext();

        Page[] existingPages = await core.Set<Page>()
            .IgnoreQueryFilters()
            .Where(predicate: found => found.AppId == appId)
            .ToArrayAsync();

        Dictionary<string, int> pageIdsByPath = existingPages
            .Where(predicate: found => !string.IsNullOrWhiteSpace(value: found.Path))
            .GroupBy(keySelector: found => NormalizePagePath(path: found.Path), comparer: StringComparer.OrdinalIgnoreCase)
            .ToDictionary(keySelector: group => group.Key, elementSelector: group => group.First().Id, comparer: StringComparer.OrdinalIgnoreCase);

        Role[] roles = await core.Set<Role>()
            .IgnoreQueryFilters()
            .Where(predicate: found => found.AppId == appId)
            .ToArrayAsync();

        Dictionary<string, Guid> roleIdsByName = roles
            .GroupBy(
                keySelector: role => role.Name,
                comparer: StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                keySelector: group => group.Key,
                elementSelector: group => group.First().Id,
                comparer: StringComparer.OrdinalIgnoreCase);

        int[] pageIds = [.. pageIdsByPath.Values.Distinct()];

        HashSet<string> existingPairs =
        [
            .. await core.Set<PageRole>()
                .IgnoreQueryFilters()
                .Where(predicate: found => pageIds.Contains(value: found.PageId))
                .Select(selector: found => found.PageId + "|" + found.RoleId)
                .ToArrayAsync()
        ];

        foreach (PageRolePackageItem item in items)
        {
            string normalizedPath = NormalizePagePath(path: item.Path);

            if (!pageIdsByPath.TryGetValue(key: normalizedPath, value: out int pageId))
            {
                throw new InvalidOperationException($"Page role target page was not found for path '{normalizedPath}'.");
            }

            if (!roleIdsByName.TryGetValue(key: item.Role, value: out Guid roleId))
            {
                throw new InvalidOperationException($"Page role target role was not found for role '{item.Role}'.");
            }

            string key = pageId + "|" + roleId;

            if (existingPairs.Contains(item: key))
            {
                continue;
            }

            bool alreadyExists = await core.Set<PageRole>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: found => found.PageId == pageId && found.RoleId == roleId);

            if (alreadyExists)
            {
                existingPairs.Add(item: key);
                continue;
            }

            existingPairs.Add(item: key);

            await core.Set<PageRole>()
                .AddAsync(entity: new PageRole
                {
                    PageId = pageId,
                    RoleId = roleId,
                });
        }

        await core.SaveChangesAsync();
    }

    private async Task ImportFolderRolesAsync(int appId, IEnumerable<PackageItem> folderRoleItems)
    {
        FolderRolePackageItem[] items = [.. folderRoleItems
            .SelectMany(selector: item => DeserializePackageItems<FolderRolePackageItem>(data: item.Data))
            .Where(predicate: item => !string.IsNullOrWhiteSpace(value: item.Path) && !string.IsNullOrWhiteSpace(value: item.Name))];

        if (items.Length == 0)
        {
            return;
        }

        await using DbContext core = coreContextFactory.CreateCoreContext();

        Folder[] existingFolders = await core.Set<Folder>()
            .IgnoreQueryFilters()
            .Where(predicate: found => found.AppId == appId)
            .ToArrayAsync();

        Dictionary<string, Folder> foldersByPath = existingFolders
            .Where(predicate: found => !string.IsNullOrWhiteSpace(value: found.Path))
            .GroupBy(keySelector: found => NormalizeFolderPath(path: found.Path), comparer: StringComparer.OrdinalIgnoreCase)
            .ToDictionary(keySelector: group => group.Key, elementSelector: group => group.First(), comparer: StringComparer.OrdinalIgnoreCase);

        string[] paths = [.. items
            .Select(selector: item => NormalizeFolderPath(path: item.Path))
            .Distinct(comparer: StringComparer.OrdinalIgnoreCase)
            .OrderBy(keySelector: path => path.Count(predicate: character => character == '/'))
            .ThenBy(keySelector: path => path, comparer: StringComparer.OrdinalIgnoreCase)];

        foreach (string path in paths)
        {
            if (foldersByPath.ContainsKey(key: path))
            {
                continue;
            }

            string parentPath = GetParentFolderPath(path: path);

            Folder folder = new()
            {
                Id = Guid.NewGuid(),
                AppId = appId,
                ParentId = !string.IsNullOrWhiteSpace(value: parentPath)
                    && foldersByPath.TryGetValue(key: parentPath, value: out Folder parent)
                        ? parent.Id
                        : null,
                Name = GetFolderName(path: path),
                Path = path,
            };

            foldersByPath[path] = folder;

            await core.Set<Folder>()
                .AddAsync(entity: folder);
        }

        await core.SaveChangesAsync();

        Role[] roles = await core.Set<Role>()
            .IgnoreQueryFilters()
            .Where(predicate: found => found.AppId == appId)
            .ToArrayAsync();

        Dictionary<string, Guid> roleIdsByName = roles
            .GroupBy(
                keySelector: role => role.Name,
                comparer: StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                keySelector: group => group.Key,
                elementSelector: group => group.First().Id,
                comparer: StringComparer.OrdinalIgnoreCase);

        Guid[] folderIds = [.. foldersByPath.Values
            .Select(selector: folder => folder.Id)
            .Distinct()];

        HashSet<string> existingPairs =
        [
            .. await core.Set<FolderRole>()
                .IgnoreQueryFilters()
                .Where(predicate: found => folderIds.Contains(value: found.FolderId))
                .Select(selector: found => found.FolderId + "|" + found.RoleId)
                .ToArrayAsync()
        ];

        foreach (FolderRolePackageItem item in items)
        {
            string normalizedPath = NormalizeFolderPath(path: item.Path);

            if (!foldersByPath.TryGetValue(key: normalizedPath, value: out Folder folder))
            {
                continue;
            }

            if (!roleIdsByName.TryGetValue(key: item.Name, value: out Guid roleId))
            {
                continue;
            }

            string key = folder.Id + "|" + roleId;

            if (!existingPairs.Add(item: key))
            {
                continue;
            }

            await core.Set<FolderRole>()
                .AddAsync(entity: new FolderRole
                {
                    FolderId = folder.Id,
                    RoleId = roleId,
                });
        }

        await core.SaveChangesAsync();
    }

    private async Task<Package> ExportAppConfigurationPackageAsync(
        int appId,
        string sourceApi)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        App app = await core.Set<App>()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(predicate: found => found.Id == appId) ?? throw new InvalidOperationException($"App '{appId}' was not found.");

        return new Package(AppConfigurationPackageName)
        {
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

    private async Task<Package> ExportPageRolesPackageAsync(
        int appId,
        string sourceApi)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        var rows = await core.Set<PageRole>()
            .IgnoreQueryFilters()
            .Join(
inner: core.Set<Page>()
    .IgnoreQueryFilters()
    .Where(predicate: found => found.AppId == appId), outerKeySelector: pageRole => pageRole.PageId, innerKeySelector: page => page.Id, resultSelector: (pageRole, page) => new { pageRole, page })
            .Join(
inner: core.Set<Role>()
    .IgnoreQueryFilters()
    .Where(predicate: found => found.AppId == appId), outerKeySelector: joined => joined.pageRole.RoleId, innerKeySelector: role => role.Id, resultSelector: (joined, role) => new PageRolePackageItem
    {
        Path = joined.page.Path,
        Role = role.Name,
    })
            .ToArrayAsync();

        PageRolePackageItem[] items = [.. rows
            .Select(selector: item => new PageRolePackageItem
            {
                Path = NormalizePagePath(path: item.Path),
                Role = item.Role,
            })
            .OrderBy(keySelector: item => item.Path, comparer: StringComparer.OrdinalIgnoreCase)
            .ThenBy(keySelector: item => item.Role, comparer: StringComparer.OrdinalIgnoreCase)];

        return new Package("PageRoles")
        {
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

        var rows = await core.Set<FolderRole>()
            .IgnoreQueryFilters()
            .Join(
inner: core.Set<Folder>()
    .IgnoreQueryFilters()
    .Where(predicate: found => found.AppId == appId), outerKeySelector: folderRole => folderRole.FolderId, innerKeySelector: folder => folder.Id, resultSelector: (folderRole, folder) => new { folderRole, folder })
            .Join(
inner: core.Set<Role>()
    .IgnoreQueryFilters()
    .Where(predicate: found => found.AppId == appId), outerKeySelector: joined => joined.folderRole.RoleId, innerKeySelector: role => role.Id, resultSelector: (joined, role) => new FolderRolePackageItem
    {
        Path = joined.folder.Path,
        Name = role.Name,
    })
            .ToArrayAsync();

        FolderRolePackageItem[] items = [.. rows
            .Select(selector: item => new FolderRolePackageItem
            {
                Path = NormalizeFolderPath(path: item.Path),
                Name = item.Name,
            })
            .OrderBy(keySelector: item => item.Path, comparer: StringComparer.OrdinalIgnoreCase)
            .ThenBy(keySelector: item => item.Name, comparer: StringComparer.OrdinalIgnoreCase)];

        return new Package("FolderRoles")
        {
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

    private async Task ImportAppConfigurationAsync(int appId, PackageItem packageItem)
    {
        AppConfigurationPackageItem imported = DeserializeAppConfiguration(data: packageItem.Data);

        if (imported is null)
        {
            return;
        }

        await using DbContext core = coreContextFactory.CreateCoreContext();

        App app = await core.Set<App>()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(predicate: found => found.Id == appId) ?? throw new InvalidOperationException($"App '{appId}' was not found.");

        app.DefaultCultureId = imported.DefaultCultureId ?? string.Empty;
        app.Name = imported.Name ?? app.Name;
        app.DefaultTheme = imported.DefaultTheme ?? app.DefaultTheme;
        app.ConfigJson = imported.ConfigJson ?? app.ConfigJson;

        await core.SaveChangesAsync();
    }

    private static AppConfigurationPackageItem DeserializeAppConfiguration(string data)
    {
        if (string.IsNullOrWhiteSpace(value: data))
        {
            return null;
        }

        using JsonDocument document = JsonDocument.Parse(json: data);
        JsonElement value = document.RootElement;

        return value.ValueKind switch
        {
            JsonValueKind.Array => value.Deserialize<AppConfigurationPackageItem[]>()
                ?.FirstOrDefault(),
            JsonValueKind.Object => value.Deserialize<AppConfigurationPackageItem>(options: JsonOptions),
            _ => null,
        };
    }

    private static T[] DeserializePackageItems<T>(string data)
    {
        if (string.IsNullOrWhiteSpace(value: data))
        {
            return [];
        }

        using JsonDocument document = JsonDocument.Parse(json: data);
        JsonElement value = document.RootElement;

        return value.ValueKind switch
        {
            JsonValueKind.Array => value.Deserialize<T[]>(options: JsonOptions) ?? [],
            JsonValueKind.Object => value.Deserialize<T>(options: JsonOptions) is T item ? [item] : [],
            _ => [],
        };
    }

    private static Package SanitizePackage(Package package) =>
        new(package.Name)
        {
            Id = package.Id,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = [.. (package.Items ?? [])
                .Select(selector: item => new PackageItem
                {
                    Id = item.Id,
                    PackageId = item.PackageId,
                    Type = item.Type,
                    Data = StripTypeMetadata(data: item.Data),
                })],
        };

    private static string StripTypeMetadata(string data)
    {
        if (string.IsNullOrWhiteSpace(value: data))
        {
            return data;
        }

        string trimmed = data.TrimStart();

        if (!trimmed.StartsWith(value: '{')
            && !trimmed.StartsWith(value: '['))
        {
            return data;
        }

        try
        {
            JsonNode node = JsonNode.Parse(json: data);

            if (node is null)
            {
                return data;
            }

            RemoveTypeMetadata(node: node);
            return node.ToJsonString();
        }
        catch (JsonException)
        {
            return data;
        }
    }

    private static void RemoveTypeMetadata(JsonNode node)
    {
        switch (node)
        {
            case JsonObject jsonObject:
                jsonObject.Remove(propertyName: "$type");

                foreach (KeyValuePair<string, JsonNode> property in jsonObject.ToArray())
                {
                    if (property.Value is not null)
                    {
                        RemoveTypeMetadata(node: property.Value);
                    }
                }

                break;

            case JsonArray jsonArray:
                foreach (JsonNode child in jsonArray)
                {
                    if (child is not null)
                    {
                        RemoveTypeMetadata(node: child);
                    }
                }

                break;
        }
    }

    private static string NormalizePagePath(string path) =>
        string.IsNullOrWhiteSpace(value: path)
            ? string.Empty
            : path.Trim()
                .Trim(trimChar: '/')
                .Replace(oldChar: '\\', newChar: '/');

    private static int GetPageDepth(string path) =>
        string.IsNullOrWhiteSpace(value: path)
            ? 0
            : path.Trim(trimChar: '/')
                .Split(separator: '/', options: StringSplitOptions.RemoveEmptyEntries).Length;

    private static string GetParentPagePath(string path)
    {
        string normalizedPath = NormalizePagePath(path: path);
        int separatorIndex = normalizedPath.LastIndexOf(value: '/');
        return separatorIndex <= 0 ? string.Empty : normalizedPath[..separatorIndex];
    }

    private static string NormalizeFolderPath(string path) =>
        string.IsNullOrWhiteSpace(value: path)
            ? string.Empty
            : path.Trim()
                .Trim(trimChar: '/')
                .Replace(oldChar: '\\', newChar: '/')
                .ToLowerInvariant();

    private static string GetParentFolderPath(string path)
    {
        string normalizedPath = NormalizeFolderPath(path: path);
        int separatorIndex = normalizedPath.LastIndexOf(value: '/');
        return separatorIndex <= 0 ? string.Empty : normalizedPath[..separatorIndex];
    }

    private static string GetFolderName(string path)
    {
        string normalizedPath = NormalizeFolderPath(path: path);
        int separatorIndex = normalizedPath.LastIndexOf(value: '/');
        return separatorIndex < 0 ? normalizedPath : normalizedPath[(separatorIndex + 1)..];
    }
}