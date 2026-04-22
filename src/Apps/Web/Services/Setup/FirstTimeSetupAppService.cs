using cCoder.ContentManagement.Exposures.Caching;
using cCoder.Core.Services.Orchestrations;
using cCoder.Data;
using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Packaging;
using cCoder.Data.Models.Security;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Web.Models;

namespace Web.Services.Setup;

internal sealed class FirstTimeSetupAppService(
    FirstTimeSetupAssetService assetService,
    ICoreContextFactory coreContextFactory,
    IServiceProvider serviceProvider)
    : IFirstTimeSetupAppService
{
    private static readonly HashSet<string> ContentManagementTypes =
    [
        "Core/Layout",
        "Core/Template",
        "Core/Page",
        "Core/PageRole",
        "Core/Component"
    ];

    private static readonly HashSet<string> CommonObjectOnlyTypes =
    [
        "Core/Resource",
        "Core/Script"
    ];

    private static readonly HashSet<string> AppScopedComponentNames =
    [
        "CoreManagement",
        "SSOMetadata",
        "SSORoleManagement",
        "SSORolePrivManagement",
        "SSORoleUserManagement"
    ];

    private static readonly HashSet<string> WorkflowTypes =
    [
        "Core/FlowDefinition",
        "Core/FlowInstanceData",
        "Workflow/FlowDefinition",
        "Workflow/FlowInstanceData"
    ];

    private static readonly HashSet<string> SchedulingTypes =
    [
        "Core/Calendar",
        "Core/CalendarEvent",
        "Core/ScheduledTask",
        "Scheduling/Calendar",
        "Scheduling/CalendarEvent",
        "Scheduling/ScheduledTask"
    ];

    public async Task<App> CreateFirstAppAsync(
        FirstTimeSetupRequest request,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        string firstAdminUserId = FirstTimeSetupIdentifiers.BuildUserId(request.Email);
        Package[] packages = assetService.LoadPackages();
        CommonObject[] commonObjects = assetService.LoadCommonObjects();
        NormalizeBaselinePackages(packages, firstAdminUserId);

        await EnsureGuestUserAsync(cancellationToken);

        IAppOrchestrationService appOrchestrationService =
            serviceProvider.GetRequiredService<IAppOrchestrationService>();

        App app = await appOrchestrationService.AddAsync(
            new App
            {
                Name = request.TenantName.Trim(),
                Domain = request.Domain,
                DefaultTheme = "Default",
                DefaultCultureId = string.Empty,
                TenantId = tenantId,
                ConfigJson = assetService.LoadDefaultAppConfig()
            });

        await PersistBaselineFoldersAsync(app.Id, packages, cancellationToken);
        await PersistBaselineDmsAssetsAsync(app.Id, firstAdminUserId, cancellationToken);
        await ImportBaselinePackagesAsync(app.Id, packages);
        await PersistImportedPageVisibilityAsync(app.Id, packages, cancellationToken);
        await PersistPackageCatalogAsync(packages, cancellationToken);
        await PersistCommonObjectsAsync(commonObjects, firstAdminUserId, cancellationToken);

        ICommonObjectCache commonObjectCache =
            serviceProvider.GetRequiredService<ICommonObjectCache>();
        IMetadataCache metadataCache =
            serviceProvider.GetRequiredService<IMetadataCache>();
        commonObjectCache.Refresh();
        metadataCache.Rebuild();

        return app;
    }

    private async Task ImportBaselinePackagesAsync(int appId, IEnumerable<Package> packages)
    {
        cCoder.Packaging.Brokers.IContentManagementPackageManagerBroker contentManagementPackageManagerBroker =
            serviceProvider.GetRequiredService<cCoder.Packaging.Brokers.IContentManagementPackageManagerBroker>();
        cCoder.Packaging.Brokers.IWorkflowPackageManagerBroker workflowPackageManagerBroker =
            serviceProvider.GetRequiredService<cCoder.Packaging.Brokers.IWorkflowPackageManagerBroker>();
        cCoder.Packaging.Brokers.ISchedulingPackageManagerBroker schedulingPackageManagerBroker =
            serviceProvider.GetRequiredService<cCoder.Packaging.Brokers.ISchedulingPackageManagerBroker>();

        foreach (Package package in packages)
        {
            string[] itemTypes = (package.Items ?? [])
                .Select(item => item.Type)
                .Where(type => !string.IsNullOrWhiteSpace(type))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            Package importPackage = CreateAppImportPackage(package);

            itemTypes = (importPackage.Items ?? [])
                .Select(item => item.Type)
                .Where(type => !string.IsNullOrWhiteSpace(type))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (itemTypes.Length == 0 || ContainsType(itemTypes, "Core/Role"))
                continue;

            if (itemTypes.Any(type => ContentManagementTypes.Contains(type)))
            {
                await contentManagementPackageManagerBroker.ImportPackageAsync(appId, importPackage);
                continue;
            }

            if (itemTypes.Any(type => WorkflowTypes.Contains(type)))
            {
                await workflowPackageManagerBroker.ImportPackageAsync(appId, importPackage);
                continue;
            }

            if (itemTypes.Any(type => SchedulingTypes.Contains(type)))
                await schedulingPackageManagerBroker.ImportPackageAsync(appId, importPackage);
        }
    }

    private async Task EnsureGuestUserAsync(CancellationToken cancellationToken)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();

        bool exists = await core.Set<User>()
            .IgnoreQueryFilters()
            .AnyAsync(user => user.Id == "Guest", cancellationToken);

        if (exists)
            return;

        await core.Set<User>().AddAsync(
            new User
            {
                Id = "Guest",
                Email = string.Empty,
                DisplayName = "Guest",
                DefaultCultureId = string.Empty,
                IsActive = true
            },
            cancellationToken);

        await core.SaveChangesAsync(cancellationToken);
    }

    private async Task PersistPackageCatalogAsync(
        IEnumerable<Package> packages,
        CancellationToken cancellationToken)
    {
        Package[] clonedPackages = packages.ToArray();

        await using DbContext core = coreContextFactory.CreateCoreContext();
        await core.Set<Package>().AddRangeAsync(clonedPackages, cancellationToken);
        await core.SaveChangesAsync(cancellationToken);
    }

    private async Task PersistBaselineFoldersAsync(
        int appId,
        IEnumerable<Package> packages,
        CancellationToken cancellationToken)
    {
        string[] paths = packages
            .SelectMany(package => package.Items ?? [])
            .Where(item => string.Equals(item.Type, "Core/FolderRole", StringComparison.OrdinalIgnoreCase))
            .SelectMany(item => ExtractFolderRolePaths(item.Data))
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path.Count(character => character == '/'))
            .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (paths.Length == 0)
            return;

        await using DbContext core = coreContextFactory.CreateCoreContext();
        Folder[] existingFolders = await core.Set<Folder>()
            .IgnoreQueryFilters()
            .Where(folder => folder.AppId == appId)
            .ToArrayAsync(cancellationToken);

        Dictionary<string, Folder> foldersByPath = existingFolders
            .Where(folder => !string.IsNullOrWhiteSpace(folder.Path))
            .ToDictionary(folder => folder.Path, StringComparer.OrdinalIgnoreCase);

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
            await core.Set<Folder>().AddAsync(folder, cancellationToken);
        }

        await core.SaveChangesAsync(cancellationToken);
    }

    private async Task PersistBaselineDmsAssetsAsync(
        int appId,
        string createdBy,
        CancellationToken cancellationToken)
    {
        string[] assetPaths = assetService.LoadDmsAssetPaths()
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path.Count(character => character is '/' or '\\'))
            .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (assetPaths.Length == 0)
            return;

        DateTimeOffset now = DateTimeOffset.UtcNow;

        await using DbContext core = coreContextFactory.CreateCoreContext();
        Folder[] existingFolders = await core.Set<Folder>()
            .IgnoreQueryFilters()
            .Where(folder => folder.AppId == appId)
            .ToArrayAsync(cancellationToken);

        Dictionary<string, Folder> foldersByPath = existingFolders
            .Where(folder => !string.IsNullOrWhiteSpace(folder.Path))
            .ToDictionary(folder => folder.Path.ToLowerInvariant(), StringComparer.OrdinalIgnoreCase);

        string[] filePaths = assetPaths
            .Select(GetBaselineDmsPath)
            .Select(path => path.ToLowerInvariant())
            .ToArray();

        cCoder.Data.Models.DMS.File[] existingFiles = await core.Set<cCoder.Data.Models.DMS.File>()
            .IgnoreQueryFilters()
            .Include(found => found.Contents)
            .Where(file => filePaths.Contains(file.Path))
            .ToArrayAsync(cancellationToken);

        Dictionary<string, cCoder.Data.Models.DMS.File> filesByPath = existingFiles
            .ToDictionary(file => file.Path, StringComparer.OrdinalIgnoreCase);

        foreach (string assetPath in assetPaths)
        {
            byte[] assetBytes = assetService.LoadAssetBytes(assetPath);
            string dmsPath = GetBaselineDmsPath(assetPath);
            string filePath = dmsPath.ToLowerInvariant();
            string folderPath = GetParentFolderPath(filePath);
            string fileName = GetFolderName(dmsPath);
            string fileSize = GetSizeOf(assetBytes);
            Folder folder = await EnsureFolderAsync(core, foldersByPath, appId, folderPath, cancellationToken);

            if (!filesByPath.TryGetValue(filePath, out cCoder.Data.Models.DMS.File file))
            {
                file = new cCoder.Data.Models.DMS.File
                {
                    Id = Guid.NewGuid(),
                    FolderId = folder.Id,
                    Folder = folder,
                    Name = fileName,
                    Path = filePath,
                    MimeType = GetMimeType(fileName),
                    Size = fileSize,
                    CreatedBy = createdBy,
                    CreatedOn = now,
                    Contents = [],
                };

                filesByPath[filePath] = file;
                await core.Set<cCoder.Data.Models.DMS.File>().AddAsync(file, cancellationToken);
            }
            else
            {
                file.FolderId = folder.Id;
                file.Folder = folder;
                file.Name = fileName;
                file.MimeType = GetMimeType(fileName);
                file.Size = fileSize;
            }

            FileContent content = file.Contents
                .OrderByDescending(found => found.Version)
                .FirstOrDefault();

            if (content is null)
            {
                file.Contents.Add(
                    new FileContent
                    {
                        Id = Guid.NewGuid(),
                        FileId = file.Id,
                        File = file,
                        Description = "Baseline DMS asset",
                        Size = fileSize,
                        CreatedBy = createdBy,
                        CreatedOn = now,
                        Version = 1,
                        RawData = assetBytes,
                    });
            }
            else
            {
                content.Description = "Baseline DMS asset";
                content.Size = fileSize;
                content.RawData = assetBytes;
                content.CreatedBy = string.IsNullOrWhiteSpace(content.CreatedBy) ? createdBy : content.CreatedBy;
                content.CreatedOn = content.CreatedOn == default ? now : content.CreatedOn;
            }
        }

        await core.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Folder> EnsureFolderAsync(
        DbContext core,
        Dictionary<string, Folder> foldersByPath,
        int appId,
        string path,
        CancellationToken cancellationToken)
    {
        string normalizedPath = path.Trim().Trim('/').ToLowerInvariant();

        if (foldersByPath.TryGetValue(normalizedPath, out Folder existingFolder))
            return existingFolder;

        string parentPath = GetParentFolderPath(normalizedPath);
        Folder parent = string.IsNullOrWhiteSpace(parentPath)
            ? null
            : await EnsureFolderAsync(core, foldersByPath, appId, parentPath, cancellationToken);

        Folder folder = new()
        {
            Id = Guid.NewGuid(),
            AppId = appId,
            ParentId = parent?.Id,
            Parent = parent,
            Name = GetFolderName(normalizedPath),
            Path = normalizedPath,
        };

        foldersByPath[normalizedPath] = folder;
        await core.Set<Folder>().AddAsync(folder, cancellationToken);

        return folder;
    }

    private static string GetBaselineDmsPath(string assetPath)
    {
        const string prefix = "Baseline/DMS/";
        string normalizedPath = assetPath.Replace('\\', '/').Trim('/');

        if (!normalizedPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"DMS baseline asset path must start with {prefix}: {assetPath}");

        return normalizedPath[prefix.Length..];
    }

    private static string GetMimeType(string fileName)
    {
        string extension = Path.GetExtension(fileName);

        return extension.ToLowerInvariant() switch
        {
            ".gif" => "image/gif",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            _ => "application/octet-stream",
        };
    }

    private async Task PersistImportedPageVisibilityAsync(
        int appId,
        IEnumerable<Package> packages,
        CancellationToken cancellationToken)
    {
        Dictionary<string, bool> visibilityByPath = packages
            .SelectMany(package => package.Items ?? [])
            .Where(item => string.Equals(item.Type, "Core/Page", StringComparison.OrdinalIgnoreCase))
            .SelectMany(item => ExtractPageVisibility(item.Data))
            .GroupBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Last().ShowOnMenus, StringComparer.OrdinalIgnoreCase);

        if (visibilityByPath.Count == 0)
            return;

        await using DbContext core = coreContextFactory.CreateCoreContext();
        Page[] pages = await core.Set<Page>()
            .IgnoreQueryFilters()
            .Where(page => page.AppId == appId)
            .ToArrayAsync(cancellationToken);

        foreach (Page page in pages)
        {
            string normalizedPath = NormalizePagePath(page.Path);
            if (visibilityByPath.TryGetValue(normalizedPath, out bool showOnMenus))
                page.ShowOnMenus = showOnMenus;
        }

        await core.SaveChangesAsync(cancellationToken);
    }

    private async Task PersistCommonObjectsAsync(
        IEnumerable<CommonObject> commonObjects,
        string createdBy,
        CancellationToken cancellationToken)
    {
        CommonObject[] items = commonObjects.ToArray();
        NormalizeCommonObjects(items, createdBy);

        await using DbContext core = coreContextFactory.CreateCoreContext();
        await core.Set<CommonObject>().AddRangeAsync(items, cancellationToken);
        await core.SaveChangesAsync(cancellationToken);
    }

    private static bool ContainsType(IEnumerable<string> itemTypes, string expectedType) =>
        itemTypes.Any(type => string.Equals(type, expectedType, StringComparison.OrdinalIgnoreCase));

    private static IEnumerable<string> ExtractFolderRolePaths(string data)
    {
        JToken token = JToken.Parse(data);
        IEnumerable<JObject> roles = token is JArray array
            ? array.OfType<JObject>()
            : token is JObject singleRole
                ? [singleRole]
                : [];

        foreach (JObject role in roles)
        {
            string path = role.Value<string>("Path")?.Trim().Trim('/') ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(path))
                yield return path;
        }
    }

    private static IEnumerable<(string Path, bool ShowOnMenus)> ExtractPageVisibility(string data)
    {
        JToken token = JToken.Parse(data);
        IEnumerable<JObject> pages = token is JArray array
            ? array.OfType<JObject>()
            : token is JObject singlePage
                ? [singlePage]
                : [];

        foreach (JObject page in pages)
        {
            string path = NormalizePagePath(page.Value<string>("Path"));
            bool showOnMenus = page.Value<bool?>("ShowOnMenus") ?? false;

            yield return (path, showOnMenus);
        }
    }

    private static string NormalizePagePath(string path) =>
        (path ?? string.Empty).Trim().TrimStart('/');

    private static string GetParentFolderPath(string path)
    {
        int separatorIndex = path.LastIndexOf('/');
        return separatorIndex <= 0 ? string.Empty : path[..separatorIndex];
    }

    private static string GetFolderName(string path)
    {
        int separatorIndex = path.LastIndexOf('/');
        return separatorIndex < 0 ? path : path[(separatorIndex + 1)..];
    }

    private static string GetSizeOf(byte[] content)
    {
        if (content.Length > 1_000_000_000)
            return $"{content.Length / 1000 / 1000 / 1000} GB";

        if (content.Length > 1_000_000)
            return $"{content.Length / 1000 / 1000} MB";

        return content.Length > 1000
            ? $"{content.Length / 1000} KB"
            : $"{content.Length} B";
    }

    private static void NormalizeCommonObjects(IEnumerable<CommonObject> commonObjects, string createdBy)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (CommonObject commonObject in commonObjects)
        {
            NormalizeDateTimeOffsetProperty(commonObject, nameof(CommonObject.CreatedOn), now);
            NormalizeDateTimeOffsetProperty(commonObject, nameof(CommonObject.LastUpdated), now);
            NormalizeStringProperty(commonObject, nameof(CommonObject.CreatedBy), createdBy);
            NormalizeStringProperty(commonObject, nameof(CommonObject.LastUpdatedBy), createdBy);
            NormalizeCommonObjectJson(commonObject, createdBy, now);
        }
    }

    private static void NormalizeBaselinePackages(IEnumerable<Package> packages, string createdBy)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (Package package in packages)
        {
            foreach (PackageItem item in package.Items ?? [])
            {
                JToken data = JToken.Parse(item.Data);
                NormalizeAuditFields(data, createdBy, now);
                item.Data = data.ToString(Formatting.None);
            }
        }
    }

    private static Package CreateAppImportPackage(Package package)
    {
        PackageItem[] items = (package.Items ?? [])
            .Select(FilterPackageItemForAppImport)
            .Where(item => item is not null)
            .ToArray()!;

        return new Package
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Category = package.Category,
            SourceApi = package.SourceApi,
            Items = items,
        };
    }

    private static PackageItem FilterPackageItemForAppImport(PackageItem item)
    {
        if (CommonObjectOnlyTypes.Contains(item.Type))
            return null;

        if (!string.Equals(item.Type, "Core/Component", StringComparison.OrdinalIgnoreCase))
            return item;

        JToken data = JToken.Parse(item.Data);
        JArray components = data is JArray array ? array : new JArray(data);
        JArray appScopedComponents = new(
            components
                .OfType<JObject>()
                .Where(component => AppScopedComponentNames.Contains(component.Value<string>("Name") ?? string.Empty))
                .Select(component => component.DeepClone()));

        if (appScopedComponents.Count == 0)
            return null;

        return new PackageItem
        {
            Id = item.Id,
            PackageId = item.PackageId,
            Type = item.Type,
            Data = appScopedComponents.ToString(Formatting.None),
        };
    }

    private static void NormalizeCommonObjectJson(CommonObject commonObject, string createdBy, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(commonObject.Json))
            return;

        JToken json = JToken.Parse(commonObject.Json);
        NormalizeAuditFields(json, createdBy, now);
        commonObject.Json = json.ToString(Formatting.None);
    }

    private static void NormalizeAuditFields(JToken token, string createdBy, DateTimeOffset now)
    {
        foreach (JObject record in EnumerateObjects(token))
        {
            if (!LooksLikeBaselineRecord(record))
                continue;

            record["CreatedBy"] = createdBy;
            record["LastUpdatedBy"] = createdBy;
            record["CreatedOn"] = NormalizeDateValue(record["CreatedOn"], now);
            record["LastUpdated"] = NormalizeDateValue(record["LastUpdated"], now);
        }
    }

    private static IEnumerable<JObject> EnumerateObjects(JToken token)
    {
        if (token is JObject record)
            yield return record;

        if (token is not JContainer container)
            yield break;

        foreach (JObject descendant in container.Descendants().OfType<JObject>())
            yield return descendant;
    }

    private static bool LooksLikeBaselineRecord(JObject record) =>
        record.ContainsKey("CreatedBy")
        || record.ContainsKey("LastUpdatedBy")
        || record.ContainsKey("CreatedOn")
        || record.ContainsKey("LastUpdated")
        || record.ContainsKey("Name")
        || record.ContainsKey("Html");

    private static JToken NormalizeDateValue(JToken value, DateTimeOffset fallbackValue) =>
        value is null
        || value.Type == JTokenType.Null
        || !DateTimeOffset.TryParse(value.ToString(), out DateTimeOffset parsed)
        || parsed == default
            ? fallbackValue
            : parsed;

    private static void NormalizeDateTimeOffsetProperty(CommonObject commonObject, string propertyName, DateTimeOffset fallbackValue)
    {
        PropertyInfo property = typeof(CommonObject).GetProperty(propertyName)!;
        object value = property.GetValue(commonObject);

        if (value is null)
        {
            property.SetValue(commonObject, fallbackValue);
            return;
        }

        if (value is DateTimeOffset dateTimeOffset && dateTimeOffset == default)
            property.SetValue(commonObject, fallbackValue);
    }

    private static void NormalizeStringProperty(CommonObject commonObject, string propertyName, string fallbackValue)
    {
        PropertyInfo property = typeof(CommonObject).GetProperty(propertyName)!;

        property.SetValue(commonObject, fallbackValue);
    }

}
