#nullable enable
using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Packaging;
using cCoder.Data.Models.Security;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace cCoder.Core.Services.Setup;

internal sealed class BaselineAssetRepairService(
    FirstTimeSetupAssetService assetService,
    ICoreContextFactory coreContextFactory,
    ILogger<BaselineAssetRepairService> logger)
{
    public async Task RepairAsync(CancellationToken cancellationToken = default)
    {
        Package[] packages = assetService.LoadPackages();
        FolderRoleSeed[] folderRoleSeeds = packages
            .SelectMany(package => package.Items ?? [])
            .Where(item => string.Equals(item.Type, "Core/FolderRole", StringComparison.OrdinalIgnoreCase))
            .SelectMany(item => ExtractFolderRoleSeeds(item.Data))
            .Distinct()
            .ToArray();

        string[] assetPaths = assetService.LoadDmsAssetPaths()
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path.Count(character => character is '/' or '\\'))
            .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (folderRoleSeeds.Length == 0 && assetPaths.Length == 0)
            return;

        await using DbContext core = coreContextFactory.CreateCoreContext();

        App[] apps = await core.Set<App>()
            .IgnoreQueryFilters()
            .OrderBy(app => app.Id)
            .ToArrayAsync(cancellationToken);

        int repairedFolderCount = 0;
        int repairedFolderRoleCount = 0;
        int repairedFileCount = 0;

        foreach (App app in apps)
        {
            (int folders, int folderRoles, int files) = await RepairAppAsync(
                core,
                app.Id,
                folderRoleSeeds,
                assetPaths,
                cancellationToken);

            repairedFolderCount += folders;
            repairedFolderRoleCount += folderRoles;
            repairedFileCount += files;
        }

        if (repairedFolderCount == 0 && repairedFolderRoleCount == 0 && repairedFileCount == 0)
        {
            logger.LogDebug("Baseline asset repair found nothing to change.");
            return;
        }

        await core.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Baseline asset repair restored {FolderCount} folders, {FolderRoleCount} folder-role bindings, and {FileCount} files.",
            repairedFolderCount,
            repairedFolderRoleCount,
            repairedFileCount);
    }

    private async Task<(int FolderCount, int FolderRoleCount, int FileCount)> RepairAppAsync(
        DbContext core,
        int appId,
        FolderRoleSeed[] folderRoleSeeds,
        string[] assetPaths,
        CancellationToken cancellationToken)
    {
        Role[] roles = await core.Set<Role>()
            .IgnoreQueryFilters()
            .Where(role => role.AppId == appId)
            .ToArrayAsync(cancellationToken);

        Dictionary<string, Role> rolesByName = roles
            .Where(role => !string.IsNullOrWhiteSpace(role.Name))
            .ToDictionary(role => role.Name, StringComparer.OrdinalIgnoreCase);

        Folder[] existingFolders = await core.Set<Folder>()
            .IgnoreQueryFilters()
            .Where(folder => folder.AppId == appId)
            .ToArrayAsync(cancellationToken);

        Dictionary<string, Folder> foldersByPath = existingFolders
            .Where(folder => !string.IsNullOrWhiteSpace(folder.Path))
            .ToDictionary(
                folder => NormalizeFolderPath(folder.Path),
                StringComparer.OrdinalIgnoreCase);

        int repairedFolderCount = 0;

        foreach (string path in folderRoleSeeds.Select(seed => seed.Path).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (await EnsureFolderAsync(core, foldersByPath, appId, path, cancellationToken) is not null)
                repairedFolderCount++;
        }

        foreach (string assetPath in assetPaths)
        {
            string dmsPath = GetBaselineDmsPath(assetPath);
            string folderPath = GetParentFolderPath(dmsPath);

            if (await EnsureFolderAsync(core, foldersByPath, appId, folderPath, cancellationToken) is not null)
                repairedFolderCount++;
        }

        Guid[] folderIds = foldersByPath.Values.Select(folder => folder.Id).Distinct().ToArray();
        Guid[] roleIds = rolesByName.Values.Select(role => role.Id).Distinct().ToArray();

        FolderRole[] existingFolderRoles = folderIds.Length == 0 || roleIds.Length == 0
            ? []
            : await core.Set<FolderRole>()
                .IgnoreQueryFilters()
                .Where(folderRole => folderIds.Contains(folderRole.FolderId) && roleIds.Contains(folderRole.RoleId))
                .ToArrayAsync(cancellationToken);

        HashSet<(Guid FolderId, Guid RoleId)> folderRoleKeys =
        [
            .. existingFolderRoles.Select(folderRole => (folderRole.FolderId, folderRole.RoleId))
        ];

        int repairedFolderRoleCount = 0;

        foreach (FolderRoleSeed seed in folderRoleSeeds)
        {
            if (!rolesByName.TryGetValue(seed.RoleName, out Role? role) || role is null)
                continue;

            if (!foldersByPath.TryGetValue(seed.Path, out Folder? folder) || folder is null)
                continue;

            if (!folderRoleKeys.Add((folder.Id, role.Id)))
                continue;

            await core.Set<FolderRole>().AddAsync(
                new FolderRole
                {
                    FolderId = folder.Id,
                    RoleId = role.Id,
                },
                cancellationToken);

            repairedFolderRoleCount++;
        }

        string[] filePaths = assetPaths
            .Select(GetBaselineDmsPath)
            .Select(NormalizeFolderPath)
            .ToArray();

        cCoder.Data.Models.DMS.File[] existingFiles = filePaths.Length == 0
            ? []
            : await core.Set<cCoder.Data.Models.DMS.File>()
                .IgnoreQueryFilters()
                .Include(file => file.Contents)
                .Where(file => filePaths.Contains(file.Path))
                .ToArrayAsync(cancellationToken);

        Dictionary<string, cCoder.Data.Models.DMS.File> filesByPath = existingFiles.ToDictionary(
            file => NormalizeFolderPath(file.Path),
            StringComparer.OrdinalIgnoreCase);

        string createdBy = await core.Set<User>()
            .IgnoreQueryFilters()
            .Where(user => user.Id != "Guest")
            .OrderBy(user => user.Id)
            .Select(user => user.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? "Guest";

        int repairedFileCount = 0;
        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (string assetPath in assetPaths)
        {
            byte[] assetBytes = assetService.LoadAssetBytes(assetPath);
            string dmsPath = NormalizeFolderPath(GetBaselineDmsPath(assetPath));
            string fileName = GetFolderName(dmsPath);
            string folderPath = GetParentFolderPath(dmsPath);
            Folder folder = foldersByPath[folderPath];

            if (!filesByPath.TryGetValue(dmsPath, out cCoder.Data.Models.DMS.File? file) || file is null)
            {
                file = new cCoder.Data.Models.DMS.File
                {
                    Id = Guid.NewGuid(),
                    FolderId = folder.Id,
                    Folder = folder,
                    Name = fileName,
                    Path = dmsPath,
                    MimeType = GetMimeType(fileName),
                    Size = GetSizeOf(assetBytes),
                    CreatedBy = createdBy,
                    CreatedOn = now,
                    Contents = [],
                };

                filesByPath[dmsPath] = file;
                await core.Set<cCoder.Data.Models.DMS.File>().AddAsync(file, cancellationToken);
                repairedFileCount++;
            }
            else
            {
                file.FolderId = folder.Id;
                file.Folder = folder;
                file.Name = fileName;
                file.Path = dmsPath;
                file.MimeType = GetMimeType(fileName);
                file.Size = GetSizeOf(assetBytes);
            }

            FileContent? latestContent = file.Contents
                .OrderByDescending(content => content.Version)
                .FirstOrDefault();

            if (latestContent is null)
            {
                file.Contents.Add(
                    new FileContent
                    {
                        Id = Guid.NewGuid(),
                        FileId = file.Id,
                        File = file,
                        Description = "Baseline DMS asset",
                        Size = file.Size,
                        CreatedBy = createdBy,
                        CreatedOn = now,
                        Version = 1,
                        RawData = assetBytes,
                    });

                continue;
            }

            latestContent.Description = "Baseline DMS asset";
            latestContent.Size = file.Size;

            if (latestContent.RawData is null || latestContent.RawData.Length == 0)
                latestContent.RawData = assetBytes;

            if (string.IsNullOrWhiteSpace(latestContent.CreatedBy))
                latestContent.CreatedBy = createdBy;

            if (latestContent.CreatedOn == default)
                latestContent.CreatedOn = now;
        }

        return (repairedFolderCount, repairedFolderRoleCount, repairedFileCount);
    }

    private static async Task<Folder?> EnsureFolderAsync(
        DbContext core,
        Dictionary<string, Folder> foldersByPath,
        int appId,
        string path,
        CancellationToken cancellationToken)
    {
        string normalizedPath = NormalizeFolderPath(path);

        if (string.IsNullOrWhiteSpace(normalizedPath))
            return null;

        if (foldersByPath.ContainsKey(normalizedPath))
            return null;

        string parentPath = GetParentFolderPath(normalizedPath);

        if (!string.IsNullOrWhiteSpace(parentPath))
            _ = await EnsureFolderAsync(core, foldersByPath, appId, parentPath, cancellationToken);

        Folder folder = new()
        {
            Id = Guid.NewGuid(),
            AppId = appId,
            ParentId = !string.IsNullOrWhiteSpace(parentPath)
                ? foldersByPath[parentPath].Id
                : null,
            Name = GetFolderName(normalizedPath),
            Path = normalizedPath,
        };

        foldersByPath[normalizedPath] = folder;
        await core.Set<Folder>().AddAsync(folder, cancellationToken);
        return folder;
    }

    private static IEnumerable<FolderRoleSeed> ExtractFolderRoleSeeds(string data)
    {
        JToken token = JToken.Parse(data);
        IEnumerable<JObject> roles = token is JArray array
            ? array.OfType<JObject>()
            : token is JObject singleRole
                ? [singleRole]
                : [];

        foreach (JObject role in roles)
        {
            string path = NormalizeFolderPath(role.Value<string>("Path"));
            string roleName = role.Value<string>("Name")?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(roleName))
                continue;

            yield return new FolderRoleSeed(path, roleName);
        }
    }

    private static string GetBaselineDmsPath(string assetPath)
    {
        const string prefix = "Baseline/DMS/";
        string normalizedPath = assetPath.Replace('\\', '/').Trim('/');

        if (!normalizedPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"DMS baseline asset path must start with {prefix}: {assetPath}");

        return normalizedPath[prefix.Length..];
    }

    private static string NormalizeFolderPath(string? path) =>
        (path ?? string.Empty).Trim().Trim('/').Replace('\\', '/').ToLowerInvariant();

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

    private static string GetMimeType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".gif" => "image/gif",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            _ => "application/octet-stream",
        };

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

    private readonly record struct FolderRoleSeed(string Path, string RoleName);
}
