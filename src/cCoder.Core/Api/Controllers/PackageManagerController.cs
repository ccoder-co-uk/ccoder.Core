using System.Text.Json;
using System.Text.Json.Nodes;
using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Security;
using cCoder.Packaging.Models;
using cCoder.Data.Models.Packaging;
using cCoder.Packaging.Services.Orchestrations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Api.Controllers;

[ODataIgnored]
[ApiController]
[Route("Api/Core/Package")]
public class PackageManagerController(
    IPackageManagerOrchestrationService packageManagerOrchestrationService,
    ICoreContextFactory coreContextFactory
) : ControllerBase
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

    [HttpGet("Export")]
    public async Task<IActionResult> Export([FromQuery] int appId, [FromQuery] string[] packageNames = null)
    {
        string[] requestedPackages =
            packageNames?.Where(packageName => !string.IsNullOrWhiteSpace(packageName)).ToArray()
            ?? [];

        if (requestedPackages.Length == 0)
            requestedPackages = DefaultPackageNames;

        List<Package> exportedPackages = [];

        foreach (string packageName in requestedPackages)
        {
            if (string.Equals(packageName, AppConfigurationPackageName, StringComparison.OrdinalIgnoreCase))
            {
                exportedPackages.Add(await ExportAppConfigurationPackageAsync(appId));
                continue;
            }

            if (string.Equals(packageName, "PageRoles", StringComparison.OrdinalIgnoreCase))
            {
                exportedPackages.Add(await ExportPageRolesPackageAsync(appId));
                continue;
            }

            if (string.Equals(packageName, "FolderRoles", StringComparison.OrdinalIgnoreCase))
            {
                exportedPackages.Add(await ExportFolderRolesPackageAsync(appId));
                continue;
            }

            exportedPackages.Add(packageManagerOrchestrationService.ExportPackage(appId, packageName));
        }

        return Ok(exportedPackages);
    }

    [HttpPost("Import")]
    public async Task<IActionResult> ImportAsync([FromQuery] int appId, [FromBody] Package package)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await ImportPackagesAsync(appId, [package]);
        return Ok();
    }

    [HttpPost("ImportThis")]
    public async Task<IActionResult> ImportThisAsync([FromQuery] int appId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        using JsonDocument document = await JsonDocument.ParseAsync(Request.Body);
        JsonElement body = document.RootElement;

        if (body.ValueKind == JsonValueKind.Array)
        {
            Package[] packages = body.Deserialize<Package[]>(JsonOptions);

            await ImportPackagesAsync(appId, packages);

            return Ok();
        }

        Package entity = body.Deserialize<Package>(JsonOptions);
        if (entity is not null)
            await ImportPackagesAsync(appId, [entity]);

        return Ok();
    }

    private async Task ImportPackagesAsync(int appId, IEnumerable<Package> packages)
    {
        foreach (Package package in packages ?? [])
        {
            Package sanitizedPackage = SanitizePackage(package);

            PackageItem[] appItems = (sanitizedPackage.Items ?? [])
                .Where(item => string.Equals(item.Type, AppConfigurationItemType, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (appItems.Length > 0)
            {
                foreach (PackageItem appItem in appItems)
                    await ImportAppConfigurationAsync(appId, appItem);
            }

            PackageItem[] remainingItems = (sanitizedPackage.Items ?? [])
                .Where(item =>
                    !string.Equals(item.Type, AppConfigurationItemType, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(item.Type, "Core/Page", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(item.Type, "Core/PageRole", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(item.Type, "Core/FolderRole", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (remainingItems.Length > 0)
            {
                await packageManagerOrchestrationService.ImportPackageAsync(
                    appId,
                    new Package(sanitizedPackage.Name)
                    {
                        Id = sanitizedPackage.Id,
                        Description = sanitizedPackage.Description,
                        Category = sanitizedPackage.Category,
                        SourceApi = sanitizedPackage.SourceApi,
                        Items = remainingItems,
                    });
            }

            PackageItem[] pageItems = (sanitizedPackage.Items ?? [])
                .Where(item => string.Equals(item.Type, "Core/Page", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (pageItems.Length > 0)
                await ImportPagesAsync(appId, pageItems);

            PackageItem[] pageRoleItems = (sanitizedPackage.Items ?? [])
                .Where(item => string.Equals(item.Type, "Core/PageRole", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (pageRoleItems.Length > 0)
                await ImportPageRolesAsync(appId, pageRoleItems);

            PackageItem[] folderRoleItems = (sanitizedPackage.Items ?? [])
                .Where(item => string.Equals(item.Type, "Core/FolderRole", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (folderRoleItems.Length > 0)
                await ImportFolderRolesAsync(appId, folderRoleItems);
        }
    }

    private async Task ImportPagesAsync(int appId, IEnumerable<PackageItem> pageItems)
    {
        Page[] items = pageItems
            .SelectMany(item => DeserializePackageItems<Page>(item.Data))
            .OrderBy(item => GetPageDepth(item.Path))
            .ThenBy(item => item.Order)
            .ToArray();

        if (items.Length == 0)
            return;

        await using DbContext core = coreContextFactory.CreateCoreContext();

        foreach (Page item in items)
        {
            string normalizedPath = NormalizePagePath(item.Path);
            string parentPath = GetParentPagePath(normalizedPath);

            Page parent = string.IsNullOrWhiteSpace(parentPath)
                ? null
                : await core.Set<Page>()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(found => found.AppId == appId && found.Path == parentPath);

            int? parentId = parent?.Id;

            Page existingPage = await core.Set<Page>()
                .IgnoreQueryFilters()
                .Include(found => found.PageInfo)
                .Include(found => found.Contents)
                .FirstOrDefaultAsync(found => found.AppId == appId && found.Path == normalizedPath);

            existingPage ??= await core.Set<Page>()
                .IgnoreQueryFilters()
                .Include(found => found.PageInfo)
                .Include(found => found.Contents)
                .FirstOrDefaultAsync(found =>
                    found.AppId == appId
                    && found.Name == item.Name
                    && found.ParentId == parentId);

            if (existingPage is null)
            {
                await core.Set<Page>().AddAsync(new Page
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
                    PageInfo = (item.PageInfo ?? [])
                        .Select(info => new PageInfo
                        {
                            CultureId = info.CultureId,
                            Title = info.Title,
                            Description = info.Description,
                            Keywords = info.Keywords,
                        })
                        .ToList(),
                    Contents = (item.Contents ?? [])
                        .Select(content => new cCoder.Data.Models.CMS.Content
                        {
                            CultureId = content.CultureId,
                            Name = content.Name,
                            Html = content.Html,
                        })
                        .ToList(),
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

            core.Set<PageInfo>().RemoveRange(existingPage.PageInfo ?? []);
            core.Set<cCoder.Data.Models.CMS.Content>().RemoveRange(existingPage.Contents ?? []);

            existingPage.PageInfo = (item.PageInfo ?? [])
                .Select(info => new PageInfo
                {
                    PageId = existingPage.Id,
                    CultureId = info.CultureId,
                    Title = info.Title,
                    Description = info.Description,
                    Keywords = info.Keywords,
                })
                .ToList();
            existingPage.Contents = (item.Contents ?? [])
                .Select(content => new cCoder.Data.Models.CMS.Content
                {
                    PageId = existingPage.Id,
                    CultureId = content.CultureId,
                    Name = content.Name,
                    Html = content.Html,
                })
                .ToList();

            await core.SaveChangesAsync();
        }
    }

    private async Task ImportPageRolesAsync(int appId, IEnumerable<PackageItem> pageRoleItems)
    {
        PageRolePackageItem[] items = pageRoleItems
            .SelectMany(item => DeserializePackageItems<PageRolePackageItem>(item.Data))
            .ToArray();

        if (items.Length == 0)
            return;

        await using DbContext core = coreContextFactory.CreateCoreContext();

        Dictionary<string, int> pageIdsByPath = await core.Set<Page>()
            .IgnoreQueryFilters()
            .Where(found => found.AppId == appId)
            .ToDictionaryAsync(found => NormalizePagePath(found.Path), found => found.Id, StringComparer.OrdinalIgnoreCase);

        Dictionary<string, Guid> roleIdsByName = await core.Set<Role>()
            .IgnoreQueryFilters()
            .Where(found => found.AppId == appId)
            .ToDictionaryAsync(found => found.Name, found => found.Id, StringComparer.OrdinalIgnoreCase);

        int[] pageIds = pageIdsByPath.Values
            .Distinct()
            .ToArray();

        HashSet<string> existingPairs =
        [
            .. await core.Set<PageRole>()
                .IgnoreQueryFilters()
                .Where(found => pageIds.Contains(found.PageId))
                .Select(found => found.PageId + "|" + found.RoleId)
                .ToArrayAsync()
        ];

        foreach (PageRolePackageItem item in items)
        {
            string normalizedPath = NormalizePagePath(item.Path);

            if (!pageIdsByPath.TryGetValue(normalizedPath, out int pageId))
                throw new InvalidOperationException($"Page role target page was not found for path '{normalizedPath}'.");

            if (!roleIdsByName.TryGetValue(item.Role, out Guid roleId))
                throw new InvalidOperationException($"Page role target role was not found for role '{item.Role}'.");

            string key = pageId + "|" + roleId;
            if (existingPairs.Contains(key))
                continue;

            bool alreadyExists = await core.Set<PageRole>()
                .IgnoreQueryFilters()
                .AnyAsync(found => found.PageId == pageId && found.RoleId == roleId);

            if (alreadyExists)
            {
                existingPairs.Add(key);
                continue;
            }

            existingPairs.Add(key);

            await core.Set<PageRole>().AddAsync(new PageRole
            {
                PageId = pageId,
                RoleId = roleId,
            });
        }

        await core.SaveChangesAsync();
    }

    private async Task ImportFolderRolesAsync(int appId, IEnumerable<PackageItem> folderRoleItems)
    {
        FolderRolePackageItem[] items = folderRoleItems
            .SelectMany(item => DeserializePackageItems<FolderRolePackageItem>(item.Data))
            .Where(item => !string.IsNullOrWhiteSpace(item.Path) && !string.IsNullOrWhiteSpace(item.Name))
            .ToArray();

        if (items.Length == 0)
            return;

        await using DbContext core = coreContextFactory.CreateCoreContext();

        Folder[] existingFolders = await core.Set<Folder>()
            .IgnoreQueryFilters()
            .Where(found => found.AppId == appId)
            .ToArrayAsync();

        Dictionary<string, Folder> foldersByPath = existingFolders
            .Where(found => !string.IsNullOrWhiteSpace(found.Path))
            .ToDictionary(found => NormalizeFolderPath(found.Path), StringComparer.OrdinalIgnoreCase);

        string[] paths = items
            .Select(item => NormalizeFolderPath(item.Path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path.Count(character => character == '/'))
            .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (string path in paths)
        {
            if (foldersByPath.ContainsKey(path))
                continue;

            string parentPath = GetParentFolderPath(path);
            Folder folder = new()
            {
                Id = Guid.NewGuid(),
                AppId = appId,
                ParentId = !string.IsNullOrWhiteSpace(parentPath)
                    && foldersByPath.TryGetValue(parentPath, out Folder parent)
                        ? parent.Id
                        : null,
                Name = GetFolderName(path),
                Path = path,
            };

            foldersByPath[path] = folder;
            await core.Set<Folder>().AddAsync(folder);
        }

        await core.SaveChangesAsync();

        Dictionary<string, Guid> roleIdsByName = await core.Set<Role>()
            .IgnoreQueryFilters()
            .Where(found => found.AppId == appId)
            .ToDictionaryAsync(found => found.Name, found => found.Id, StringComparer.OrdinalIgnoreCase);

        Guid[] folderIds = foldersByPath.Values
            .Select(folder => folder.Id)
            .Distinct()
            .ToArray();

        HashSet<string> existingPairs =
        [
            .. await core.Set<FolderRole>()
                .IgnoreQueryFilters()
                .Where(found => folderIds.Contains(found.FolderId))
                .Select(found => found.FolderId + "|" + found.RoleId)
                .ToArrayAsync()
        ];

        foreach (FolderRolePackageItem item in items)
        {
            string normalizedPath = NormalizeFolderPath(item.Path);

            if (!foldersByPath.TryGetValue(normalizedPath, out Folder folder))
                continue;

            if (!roleIdsByName.TryGetValue(item.Name, out Guid roleId))
                continue;

            string key = folder.Id + "|" + roleId;
            if (!existingPairs.Add(key))
                continue;

            await core.Set<FolderRole>().AddAsync(new FolderRole
            {
                FolderId = folder.Id,
                RoleId = roleId,
            });
        }

        await core.SaveChangesAsync();
    }

    private async Task<Package> ExportAppConfigurationPackageAsync(int appId)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        App app = await core.Set<App>()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(found => found.Id == appId);

        if (app is null)
            throw new InvalidOperationException($"App '{appId}' was not found.");

        return new Package(AppConfigurationPackageName)
        {
            Description = "Application shell configuration",
            Category = "Core",
            SourceApi = $"{Request.Scheme}://{Request.Host}",
            Items =
            [
                new PackageItem
                {
                    Type = AppConfigurationItemType,
                    Data = JsonSerializer.Serialize(new AppConfigurationPackageItem
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

    private async Task<Package> ExportPageRolesPackageAsync(int appId)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        var rows = await core.Set<PageRole>()
            .IgnoreQueryFilters()
            .Join(
                core.Set<Page>().IgnoreQueryFilters().Where(found => found.AppId == appId),
                pageRole => pageRole.PageId,
                page => page.Id,
                (pageRole, page) => new { pageRole, page })
            .Join(
                core.Set<Role>().IgnoreQueryFilters().Where(found => found.AppId == appId),
                joined => joined.pageRole.RoleId,
                role => role.Id,
                (joined, role) => new PageRolePackageItem
                {
                    Path = joined.page.Path,
                    Role = role.Name,
                })
            .ToArrayAsync();

        PageRolePackageItem[] items = rows
            .Select(item => new PageRolePackageItem
            {
                Path = NormalizePagePath(item.Path),
                Role = item.Role,
            })
            .OrderBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Role, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new Package("PageRoles")
        {
            Description = "Generated by App export.",
            Category = "Dynamic",
            SourceApi = $"{Request.Scheme}://{Request.Host}/Api/",
            Items =
            [
                new PackageItem
                {
                    Type = "Core/PageRole",
                    Data = JsonSerializer.Serialize(items),
                },
            ],
        };
    }

    private async Task<Package> ExportFolderRolesPackageAsync(int appId)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        var rows = await core.Set<FolderRole>()
            .IgnoreQueryFilters()
            .Join(
                core.Set<Folder>().IgnoreQueryFilters().Where(found => found.AppId == appId),
                folderRole => folderRole.FolderId,
                folder => folder.Id,
                (folderRole, folder) => new { folderRole, folder })
            .Join(
                core.Set<Role>().IgnoreQueryFilters().Where(found => found.AppId == appId),
                joined => joined.folderRole.RoleId,
                role => role.Id,
                (joined, role) => new FolderRolePackageItem
                {
                    Path = joined.folder.Path,
                    Name = role.Name,
                })
            .ToArrayAsync();

        FolderRolePackageItem[] items = rows
            .Select(item => new FolderRolePackageItem
            {
                Path = NormalizeFolderPath(item.Path),
                Name = item.Name,
            })
            .OrderBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new Package("FolderRoles")
        {
            Description = "Generated by App export.",
            Category = "Dynamic",
            SourceApi = $"{Request.Scheme}://{Request.Host}/Api/",
            Items =
            [
                new PackageItem
                {
                    Type = "Core/FolderRole",
                    Data = JsonSerializer.Serialize(items),
                },
            ],
        };
    }

    private async Task ImportAppConfigurationAsync(int appId, PackageItem packageItem)
    {
        AppConfigurationPackageItem imported = DeserializeAppConfiguration(packageItem.Data);
        if (imported is null)
            return;

        await using DbContext core = coreContextFactory.CreateCoreContext();

        App app = await core.Set<App>()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(found => found.Id == appId);

        if (app is null)
            throw new InvalidOperationException($"App '{appId}' was not found.");

        app.DefaultCultureId = imported.DefaultCultureId ?? string.Empty;
        app.Name = imported.Name ?? app.Name;
        app.DefaultTheme = imported.DefaultTheme ?? app.DefaultTheme;
        app.ConfigJson = imported.ConfigJson ?? app.ConfigJson;

        await core.SaveChangesAsync();
    }

    private static AppConfigurationPackageItem DeserializeAppConfiguration(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
            return null;

        using JsonDocument document = JsonDocument.Parse(data);
        JsonElement value = document.RootElement;

        return value.ValueKind switch
        {
            JsonValueKind.Array => value.Deserialize<AppConfigurationPackageItem[]>()
                ?.FirstOrDefault(),
            JsonValueKind.Object => value.Deserialize<AppConfigurationPackageItem>(JsonOptions),
            _ => null,
        };
    }

    private static T[] DeserializePackageItems<T>(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
            return [];

        using JsonDocument document = JsonDocument.Parse(data);
        JsonElement value = document.RootElement;

        return value.ValueKind switch
        {
            JsonValueKind.Array => value.Deserialize<T[]>(JsonOptions) ?? [],
            JsonValueKind.Object => value.Deserialize<T>(JsonOptions) is T item ? [item] : [],
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
            Items = (package.Items ?? [])
                .Select(item => new PackageItem
                {
                    Id = item.Id,
                    PackageId = item.PackageId,
                    Type = item.Type,
                    Data = StripTypeMetadata(item.Data),
                })
                .ToArray(),
        };

    private static string StripTypeMetadata(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
            return data;

        string trimmed = data.TrimStart();
        if (!trimmed.StartsWith("{", StringComparison.Ordinal) && !trimmed.StartsWith("[", StringComparison.Ordinal))
            return data;

        try
        {
            JsonNode node = JsonNode.Parse(data);
            if (node is null)
                return data;

            RemoveTypeMetadata(node);
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
                jsonObject.Remove("$type");

                foreach (KeyValuePair<string, JsonNode> property in jsonObject.ToArray())
                {
                    if (property.Value is not null)
                        RemoveTypeMetadata(property.Value);
                }

                break;

            case JsonArray jsonArray:
                foreach (JsonNode child in jsonArray)
                {
                    if (child is not null)
                        RemoveTypeMetadata(child);
                }

                break;
        }
    }

    private static string NormalizePagePath(string path) =>
        string.IsNullOrWhiteSpace(path)
            ? string.Empty
            : path.Trim().Trim('/').Replace('\\', '/');

    private static int GetPageDepth(string path) =>
        string.IsNullOrWhiteSpace(path)
            ? 0
            : path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).Length;

    private static string GetParentPagePath(string path)
    {
        string normalizedPath = NormalizePagePath(path);
        int separatorIndex = normalizedPath.LastIndexOf('/');
        return separatorIndex <= 0 ? string.Empty : normalizedPath[..separatorIndex];
    }

    private static string NormalizeFolderPath(string path) =>
        string.IsNullOrWhiteSpace(path)
            ? string.Empty
            : path.Trim().Trim('/').Replace('\\', '/').ToLowerInvariant();

    private static string GetParentFolderPath(string path)
    {
        string normalizedPath = NormalizeFolderPath(path);
        int separatorIndex = normalizedPath.LastIndexOf('/');
        return separatorIndex <= 0 ? string.Empty : normalizedPath[..separatorIndex];
    }

    private static string GetFolderName(string path)
    {
        string normalizedPath = NormalizeFolderPath(path);
        int separatorIndex = normalizedPath.LastIndexOf('/');
        return separatorIndex < 0 ? normalizedPath : normalizedPath[(separatorIndex + 1)..];
    }
}

internal sealed class AppConfigurationPackageItem
{
    public int Id { get; init; }

    public string DefaultCultureId { get; init; }

    public string TenantId { get; init; }

    public string Name { get; init; }

    public string Domain { get; init; }

    public string DefaultTheme { get; init; }

    public string ConfigJson { get; init; }
}

internal sealed class PageRolePackageItem
{
    public string Path { get; init; }

    public string Role { get; init; }
}

internal sealed class FolderRolePackageItem
{
    public string Path { get; init; }

    public string Name { get; init; }
}
