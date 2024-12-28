using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Extensions;
using cCoder.Core.Services.Events;
using cCoder.Core.Services.Events.DMS_Moves;
using cCoder.Core.Services.Events.DMS_Moves.Value_Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Security;
using System.Text;
using File = cCoder.Core.Objects.Entities.DMS.File;

namespace cCoder.Core.Services;

public class DMSInstance(App app, ICoreDataContext db, IEventService eventService, ILogger log)
{
    public DMSResult GetFilesZipped(IEnumerable<Objects.Path> paths)
    {
        using MemoryStream result = new();
        using (ZipArchive zip = new(result, ZipArchiveMode.Create))
            paths.ForEach(path =>
            {
                if (!path.IsToFile)
                {
                    Folder folder = db.GetAll<Folder>(false).FirstOrDefault(f => f.AppId == app.Id && f.Path == path.Lowered);

                    if (folder == null)
                    {
                        log.LogWarning($"User can't see a folder @ path {path.Lowered} in DMS for app {app.Id}");
                        throw new SecurityException("Access Denied!");
                    }
                    else
                        zip.AddFolder(folder, db);
                }
                else
                {
                    File file = db.GetAll<File>(false).FirstOrDefault(f => f.Folder.AppId == app.Id && f.Path.ToLower() == path.Lowered);

                    if (file == null)
                    {
                        log.LogWarning($"User can't see a file @ path {path.Lowered} in DMS for app {app.Id}");
                        throw new SecurityException("Access Denied!");
                    }
                    else
                        zip.AddFile(file, db);
                }
            });
        return new DMSResult
        {
            MimeType = "application/zip",
            Data = new MemoryStream(result.ToArray())
        };
    }

    public DMSResult Get(Objects.Path path, int version = 0, string search = "")
    {
        if (path.IsToFile)
        {
            File file = db.GetAll<File>(false)
                .FirstOrDefault(f => f.Folder.AppId == app.Id && f.Path.ToLower() == path.Lowered);

            if (file == null)
            {
                log.LogWarning($"User can't see a file @ path {path.Lowered} in DMS for app {app.Id}");
                throw new SecurityException("Access Denied!");
            }
            else
            {
                byte[] data = db.GetAll<FileContent>(false)
                    .IgnoreQueryFilters()
                    .Where(fc => fc.FileId == file.Id)
                    .OrderByDescending(fc => fc.Version)
                    .Select(f => f.RawData)
                    .First();

                return new DMSResult
                {
                    MimeType = file.MimeType,
                    Data = new MemoryStream(data)
                };
            }
        }
        else
        {
            Folder folder = db.GetAll<Folder>(false).FirstOrDefault(f => f.AppId == app.Id && f.Path == path.Lowered);

            if (folder == null)
            {
                log.LogWarning($"User can't see a folder @ path {path.Lowered} in DMS for app {app.Id}");
                throw new SecurityException("Access Denied!");
            }
            else
            {
                using MemoryStream result = new();
                using (ZipArchive zip = new(result, ZipArchiveMode.Create))
                    _ = zip.AddFolder(folder, db, search: search);

                return new DMSResult
                {
                    MimeType = "application/zip",
                    Data = new MemoryStream(result.ToArray())
                };
            }
        }
    }

    public IEnumerable<File> Search(string needle) =>
        db.GetAll<File>(false)
            .Where(f => f.Folder.AppId == app.Id && f.Contents.Any(c => c.RawData.SequenceEqual(Encoding.UTF8.GetBytes(needle))));

    public async Task Unpack(Objects.Path path, Stream content, bool ignoreArchiveRoot = false)
    {
        log.LogInformation($"Unpacking archive to {path.FullPath}");
        Folder folder = await BuildPath(path);

        if (!(db.User.IsAdminOfApp(app.Id) || folder.UserCan(db.User, "file_create")))
        {
            log.LogWarning($"User can't create a file in {folder.Path.ToLower()} in DMS for app {app.Id}");
            throw new SecurityException("Access Denied!");
        }

        ZipArchive archive = new(content, ZipArchiveMode.Read);
        string destinationPath;

        ZipArchiveEntry rootEntry = archive.Entries
            .OrderBy(e => e.FullName.Split('/').Length)
            .First();

        string ignoreSegment = rootEntry.FullName;

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            using Stream entryStream = entry.Open();

            destinationPath = ignoreArchiveRoot
                ? $"{path.FullPath}/{entry.FullName}".Replace(ignoreSegment, "")
                : $"{path.FullPath}/{entry.FullName}";

            if (path.Lowered != destinationPath.ToLower())
            {
                log.LogInformation($"   Unpacking entry {entry.FullName} to {destinationPath}");
                await Save(new Objects.Path(destinationPath), entryStream);
            }

            entryStream.Close();
        }
    }

    public async Task Save(Objects.Path path, Stream content = null)
    {
        if (path.IsToFile)
        {
            File existingFile = db.GetAll<File>(false).FirstOrDefault(f => f.Folder.AppId == app.Id && f.Path.ToLower() == path.Lowered);
            byte[] rawBytes = content?.ToArray() ?? Array.Empty<byte>();
            Folder folder = await BuildPath(path.ParentPath);

            if (existingFile == null)
                await CreateNewFile(path, rawBytes, folder);
            else
                await UpdateFile(existingFile, rawBytes, folder);
        }
        else
            _ = await BuildPath(path);
    }

    private async Task UpdateFile(File existingFile, byte[] rawBytes, Folder folder)
    {
        if (!(db.User.IsAdminOfApp(app.Id) || folder.UserCan(db.User, "file_update")))
        {
            log.LogWarning($"User can't create a file in folder {folder.Path.ToLower()} in DMS for app {app.Id}");
            throw new SecurityException("Access Denied!");
        }
        else
            await SaveFileVersion(existingFile, rawBytes);
    }

    private async Task CreateNewFile(Objects.Path path, byte[] rawBytes, Folder folder)
    {
        if (!(db.User.IsAdminOfApp(app.Id) || folder.UserCan(db.User, "file_create")))
        {
            log.LogWarning($"User can't create a file in folder {folder.Path.ToLower()} in DMS for app {app.Id}");
            throw new SecurityException("Access Denied!");
        }
        else
            await CreateFile(path, rawBytes, folder);
    }

    private async Task SaveFileVersion(File existingFile, byte[] rawBytes)
    {
        int version = db.GetAll<FileContent>(false)
            .Where(fc => fc.FileId == existingFile.Id).OrderByDescending(fc => fc.Version)
            .Select(fc => fc.Version)
            .First() + 1;

        _ = await db.AddAsync(new FileContent
        {
            CreatedBy = db.AuthInfo.SSOUserId,
            CreatedOn = DateTimeOffset.UtcNow,
            FileId = existingFile.Id,
            Version = version,
            Size = GetSizeOf(rawBytes),
            RawData = rawBytes
        });
    }

    private async Task CreateFile(Objects.Path path, byte[] rawBytes, Folder folder)
    {
        File fileObject = new()
        {
            CreatedBy = db.User.Id,
            CreatedOn = DateTimeOffset.UtcNow,
            Name = path.Name,
            Path = path.Lowered,
            Folder = folder,
            MimeType = path.MimeType,
            Size = GetSizeOf(rawBytes),
            Contents = new[]
             {
                new FileContent
                {
                    CreatedBy = db.User.Id,
                    CreatedOn = DateTimeOffset.UtcNow,
                    Version = 1,
                    Size = GetSizeOf(rawBytes),
                    RawData = rawBytes
                }
            }
        };

        _ = await db.AddAsync(fileObject);
    }

    private static string GetSizeOf(byte[] content)
    {
        if (content.Length > 1000000000)
            return $"{content.Length / 1000 / 1000 / 1000} GB";

        if (content.Length > 1000000)
            return $"{content.Length / 1000 / 1000} MB";

        return content.Length > 1000
            ? $"{content.Length / 1000} KB"
            : $"{content.Length} B";
    }

    public async Task Drop(Objects.Path path, int version = 0)
    {
        if (path.IsToFile)
            await DropFile(path, version);
        else
            await DropFolder(path);
    }

    public async Task Move(Objects.Path oldPath, Objects.Path newPath)
    {
        Folder newParent = !string.IsNullOrEmpty(newPath.ParentPath.Lowered)
            ? db.GetAll<Folder>(false).FirstOrDefault(f => f.AppId == app.Id && f.Path == newPath.ParentPath.Lowered)
            : null;

        Folder oldParent = !string.IsNullOrEmpty(oldPath.ParentPath.Lowered)
            ? db.GetAll<Folder>(false).FirstOrDefault(f => f.AppId == app.Id && f.Path == oldPath.ParentPath.Lowered)
            : null;

        bool userIsAdmin = db.User.IsAdminOfApp(app.Id);

        if (oldPath.IsToFile && newParent == null && !newPath.IsToFile)
            newParent = await BuildPath(newPath);

        if (oldPath.IsToFile && newParent == null && newPath.IsToFile)
            newParent = await BuildPath(newPath.ParentPath);

        if (oldPath.IsToFile)
            await MoveFile(oldPath, newPath, newParent, oldParent, userIsAdmin);
        else
            await MoveFolder(oldPath, new Objects.Path(newParent != null ? $"{newParent.Path}/{newPath.Name}" : newPath.Name), app);
    }

    private async Task MoveFile(Objects.Path oldPath, Objects.Path newPath, Folder newParent, Folder oldParent, bool userIsAdmin)
    {
        ConfirmUserCanMoveFile(oldPath, newPath, newParent, oldParent, userIsAdmin);

        File movedFile = db.GetAll<File>(false)
            .Include(f => f.Folder)
                .ThenInclude(f => f.Roles)
                    .ThenInclude(fr => fr.Role)
            .FirstOrDefault(f => f.Folder.AppId == app.Id && f.Path == oldPath.Lowered);

        if (movedFile == null)
        {
            log.LogWarning($"User can't see a file @ path {oldPath.Lowered} in DMS for app {app.Id}");
            throw new SecurityException("Access Denied!");
        }

        File existingFile = db.GetAll<File>(false)
            .Where(f => f.Folder.AppId == app.Id && f.Path == newPath.Lowered)
            .FirstOrDefault();

        if (existingFile != default)
        {
            await eventService.RaiseEventAsync<FileMovedToExistingFileEvent, FileMovedToExistingFileVO>(
                new FileMovedToExistingFileEvent 
                { 
                    Subject = new FileMovedToExistingFileVO() 
                    {
                        ExistingFile = existingFile, 
                        MovedFile = movedFile 
                    }
                });
        }
        else
            //DMS/Test.txt?moveTo=Content/
            if (!newPath.IsToFile)
            {
                Folder newPathFolder = await BuildPath(newPath);

                await eventService.RaiseEventAsync<FileMovedToExistingFolderEvent, FileMovedToExistingFolderVO>(new FileMovedToExistingFolderEvent
                {
                    Subject = new FileMovedToExistingFolderVO()
                    {
                        DestinationFolder = newPathFolder,
                        DesiredPath = new Objects.Path($"{newPathFolder.Path}/{oldPath.Name}"),
                        File = movedFile
                    }
                });

            }
            else
            {
                await eventService.RaiseEventAsync<FileMovedToExistingFolderEvent, FileMovedToExistingFolderVO>(new FileMovedToExistingFolderEvent
                {
                    Subject = new FileMovedToExistingFolderVO()
                    {
                        DesiredPath = newPath,
                        DestinationFolder = newParent,
                        File = movedFile
                    }
                });

            }
    }

    private void ConfirmUserCanMoveFile(Objects.Path oldPath, Objects.Path newPath, Folder newParent, Folder oldParent, bool userIsAdmin)
    {
        if (!(userIsAdmin || (oldParent?.UserCan(db.User, "file_update") ?? false)))
        {
            log.LogWarning($"User can't update a file in folder {oldPath.ParentPath.Lowered} in DMS for app {app.Id}");
            throw new SecurityException("Access Denied!");
        }

        if (!(userIsAdmin || (newParent?.UserCan(db.User, "file_update") ?? false)))
        {
            log.LogWarning($"User can't update a file in folder {newPath.ParentPath.Lowered} in DMS for app {app.Id}");
            throw new SecurityException("Access Denied!");
        }
    }

    private async Task MoveFolder(Objects.Path oldPath, Objects.Path newPath, App app)
    {
        Folder newParent =
            db.GetAll<Folder>(false)
                .FirstOrDefault(f => f.AppId == app.Id && f.Path.ToLower() == newPath.Lowered);

        if (newParent == null)
            newParent = await BuildPath(newPath);

        Folder oldParent = !string.IsNullOrEmpty(oldPath.ParentPath.Lowered)
            ? db.GetAll<Folder>().FirstOrDefault(f => f.AppId == app.Id && f.Path == oldPath.ParentPath.Lowered)
            : null;
        bool userIsAdmin = db.User.IsAdminOfApp(app.Id) && db.User.Can(app.Id, "folder_update");

        if (!(userIsAdmin || (oldParent?.UserCan(db.User, "folder_update") ?? false)))
        {
            log.LogWarning($"User can't update folder {oldPath.ParentPath.Lowered} in DMS for app {app.Id}");
            throw new SecurityException("Access Denied!");
        }

        if (!(userIsAdmin || (newParent?.UserCan(db.User, "folder_update") ?? false)))
        {
            log.LogWarning($"User can't update folder {newPath.ParentPath.Lowered} in DMS for app {app.Id}");
            throw new SecurityException("Access Denied!");
        }

        var sourceFolder = db
            .GetAll<Folder>(false)
            .First(f => f.AppId == app.Id && f.Path.ToLower() == oldPath.Lowered);

        var newFolder = await BuildPath(new Objects.Path(newPath.Lowered + "/" + oldPath.Name));

        await eventService.RaiseEventAsync<FolderMovedToExistingFolderEvent, FolderMovedToExistingFolderVO>(new FolderMovedToExistingFolderEvent
        {
            Subject = new FolderMovedToExistingFolderVO()
            {
                DestinationFolder = newFolder,
                SourceFolder = sourceFolder
            }
        });
    }

    private async Task DropFile(Objects.Path path, int version)
    {
        log.LogDebug("Dropping file " + path);
        File file = db.GetAll<File>(true)
            .Include(f => f.Folder)
                .ThenInclude(f => f.Roles)
                    .ThenInclude(fr => fr.Role)
            .FirstOrDefault(f => f.Folder.AppId == app.Id && f.Path.ToLower() == path.Lowered);

        if (file != null && file.UserCan(db.User, "file_delete"))
            if (version != 0) // drop the specified version of the file
                await DropFileVersion(version, file);
            else // drop the file (all versions)
                await eventService.RaiseEventAsync<FileDeletedEvent, File>(new FileDeletedEvent { Subject = file });
        else
        {
            log.LogWarning($"User can't delete a file in folder {path.Lowered} in DMS for app {app.Id}");
            throw new SecurityException("Access Denied!");
        }
    }

    private async Task DropFileVersion(int version, File file)
    {
        FileContent versionedContent = db.GetAll<FileContent>().FirstOrDefault(fc => fc.FileId == file.Id && fc.Version == version);
        versionedContent.File = file;

        if (versionedContent != null)
        {
            await eventService.RaiseEventAsync<FileContentDeletedEvent, FileContent>(new FileContentDeletedEvent
            { 
                Subject = versionedContent 
            });
        }
        else
        {
            log.LogWarning($"User can't delete a file in folder {file.Folder.Path.ToLower()} in DMS for app {app.Id}");
            throw new SecurityException("Access Denied!");
        }
    }

    private async Task DropFolder(Objects.Path path)
    {
        log.LogDebug("Dropping folder " + path);
        Folder folder = db.GetAll<Folder>()
            .Include(f => f.Roles)
                .ThenInclude(fr => fr.Role)
            .FirstOrDefault(f => f.AppId == app.Id && f.Path.ToLower() == path.Lowered);

        if (folder != null && folder.UserCan(db.User, "folder_delete"))
            await eventService.RaiseEventAsync<FolderDeletedEvent, Folder>(new FolderDeletedEvent { Subject = folder });
        else
        {
            log.LogWarning($"User can't delete a folder @ path {path.Lowered} in DMS for app {app.Id}");
            throw new SecurityException("Access Denied!");
        }
    }

    private async Task<Folder> BuildPath(Objects.Path folderPath)
    {
        if (folderPath.Length > 0)
        {
            Folder existingFolder = db.GetAll<Folder>()
                .IgnoreQueryFilters()//We need to ignore query filters to prevent weirdness
                .Include(f => f.Roles)
                    .ThenInclude(fr => fr.Role)
                .FirstOrDefault(f => f.AppId == app.Id && f.Path.ToLower() == folderPath.Lowered);

            if (existingFolder == null)
                existingFolder = await CreateFolder(folderPath);

            bool canSee = db.GetAll<Folder>()
                .Any(f => f.Id == existingFolder.Id);

            if (!canSee)
                throw new SecurityException("Access Denied!");

            return existingFolder;
        }
        else
            return null;
    }

    private async Task<Folder> CreateFolder(Objects.Path folderPath)
    {
        Folder parentFolder = folderPath.ParentPath.Depth > 0 ? await BuildPath(folderPath.ParentPath) : null;

        bool userCanCreateInApp = db.User.IsAdminOfApp(app.Id) && db.User.Can(app.Id, "folder_create");
        bool userCanCreateFolderInParentFolder = parentFolder != null && parentFolder.UserCan(db.User, "folder_create");

        if (!userCanCreateInApp && !userCanCreateFolderInParentFolder)
        {
            log.LogWarning($"User can't create folder {folderPath.Lowered} in DMS for app {app.Id}");
            throw new SecurityException("Access Denied!");
        }

        log.LogDebug("   Building path: " + folderPath.FullPath);
        List<FolderRole> folderRoles = parentFolder != null
            ? parentFolder.Roles.Select(pr => new FolderRole { RoleId = pr.RoleId }).ToList()
            : Array.Empty<FolderRole>().ToList();

        Folder folder = db.GetAll<Folder>().FirstOrDefault(f => f.AppId == app.Id && f.Path.ToLower() == folderPath.Lowered)
            ??
            new Folder
            {
                Id = Guid.Empty,
                AppId = app.Id,
                Name = folderPath.Name,
                Parent = parentFolder,
                Path = folderPath.Lowered,
                Roles = folderRoles
            };

        if (folder.Id == Guid.Empty)
            folder = await db.AddAsync(folder);

        return folder;
    }
}
