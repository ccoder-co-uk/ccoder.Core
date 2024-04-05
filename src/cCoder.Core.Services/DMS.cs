using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using File = cCoder.Core.Objects.Entities.DMS.File;

namespace cCoder.Core
{
    public class DMS
    {
        readonly ILogger log;
        readonly App app;
        readonly ICoreDataContext db;

        public DMS(App app, ICoreDataContext db, ILogger log)
        {
            this.app = app;
            this.db = db;
            this.log = log;
        }

        public DMSResult GetFilesZipped(IEnumerable<Objects.Path> paths)
        {
            using MemoryStream result = new();
            using (ZipArchive zip = new(result, ZipArchiveMode.Create))
            {
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
            }
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

            var rootEntry = archive.Entries
                .OrderBy(e => e.FullName.Split('/').Length)
                .First();

            var ignoreSegment = rootEntry.FullName;

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
                File existingFile = db.GetAll<File>().FirstOrDefault(f => f.Folder.AppId == app.Id && f.Path.ToLower() == path.Lowered);
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

        static string GetSizeOf(byte[] content)
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

        async Task MoveFile(Objects.Path oldPath, Objects.Path newPath, Folder newParent, Folder oldParent, bool userIsAdmin)
        {
            ConfirmUserCanMoveFile(oldPath, newPath, newParent, oldParent, userIsAdmin);

            File sourceFile = db.GetAll<File>(true)
                .Include(f => f.Contents)
                .Include(f => f.Folder)
                .ThenInclude(f => f.Roles)
                .ThenInclude(fr => fr.Role)
                .FirstOrDefault(f => f.Folder.AppId == app.Id && f.Path == oldPath.Lowered);

            if (sourceFile == null)
            {
                log.LogWarning($"User can't see a file @ path {oldPath.Lowered} in DMS for app {app.Id}");
                throw new SecurityException("Access Denied!");
            }

            var destinationFileId = db.GetAll<File>(false)
                .Where(f => f.Folder.AppId == app.Id && f.Path == newPath.Lowered)
                .Select(c => c.Id)
                .FirstOrDefault();

            if (destinationFileId != Guid.Empty)
            {
                //DMS/Test.txt?moveTo=Test2.txt and Test2 already exists
                var latestContentVersion = db.GetAll<FileContent>(false)
                    .Where(fc => fc.FileId == destinationFileId)
                    .OrderByDescending(fc => fc.Version)
                    .Select(fc => fc.Version)
                    .First();

                var destinationFile = db.GetAll<File>(true)
                    .Include(f => f.Folder)
                    .FirstOrDefault(f => f.Folder.AppId == app.Id && f.Path == newPath.Lowered);

                destinationFile.Contents = sourceFile.Contents
                    .Select(c =>
                    {
                        var newFileContent = new FileContent().UpdateFrom(c);
                        newFileContent.Id = Guid.Empty;
                        newFileContent.Version += latestContentVersion;
                        newFileContent.FileId = destinationFileId;
                        newFileContent.RawData = c.RawData;
                        newFileContent.Size = c.Size;
                        newFileContent.File = destinationFile;
                        return newFileContent;
                    })
                    .ToList();

                await db.AddAllAsync(destinationFile.Contents);
                await Drop(oldPath);
            }
            else
            {
                if (!newPath.IsToFile)
                {
                    //DMS/Test.txt?moveTo=Content/
                    Folder newPathFolder = await BuildPath(newPath);
                    await MoveFile(oldPath, new Objects.Path($"{newPath}/{oldPath.Name}"), newPathFolder, oldParent, userIsAdmin);
                }
                else
                {
                    //DMS/Test.txt?moveTo=Content/Test2.txt
                    sourceFile.FolderId = newParent.Id;
                    sourceFile.Name = newPath.Name;
                    sourceFile.Path = $"{newParent.Path}/{newPath.Name}".ToLower();
                    _ = await db.UpdateAsync(sourceFile);
                }
            }
        }

        void ConfirmUserCanMoveFile(Objects.Path oldPath, Objects.Path newPath, Folder newParent, Folder oldParent, bool userIsAdmin)
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

        async Task MoveFolder(Objects.Path oldPath, Objects.Path newPath, App app)
        {
            Folder newParent = await BuildPath(newPath);
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

            Folder folder = db.GetAll<Folder>()
                .Include(f => f.SubFolders)
                .Include(f => f.Files)
                .First(f => f.AppId == app.Id && f.Path == oldPath.Lowered);

            folder.ParentId = newParent?.Id;
            folder.Parent = newParent;
            folder.Name = oldPath.Name;
            folder.RecomputePaths();
            await Task.WhenAll(folder.SubFolders.Select(async (sf) => await MoveFolder(new Objects.Path(sf.Path), new Objects.Path($"{folder.Path}/{sf.Name}"), app)));
            folder.Files.ForEach((f) => f.RecomputePath());
        }

        async Task DropFile(Objects.Path path, int version)
        {
            log.LogDebug("Dropping file " + path);
            File file = db.GetAll<File>(true)
                .Include(f => f.Folder)
                    .ThenInclude(f => f.Roles)
                        .ThenInclude(fr => fr.Role)
                .FirstOrDefault(f => f.Folder.AppId == app.Id && f.Path.ToLower() == path.Lowered);

            if (file != null && file.UserCan(db.User, "file_delete"))
            {
                if (version != 0) // drop the specified version of the file
                    await DropFileVersion(version, file);
                else // drop the file (all versions)
                    db.DeleteFile(file.Id);
            }
            else
            {
                log.LogWarning($"User can't delete a file in folder {path.Lowered} in DMS for app {app.Id}");
                throw new SecurityException("Access Denied!");
            }
        }


        async Task DropFileVersion(int version, File file)
        {
            FileContent versionedContent = db.GetAll<FileContent>().FirstOrDefault(fc => fc.FileId == file.Id && fc.Version == version);
            if (versionedContent != null)
            {
                _ = await db.DeleteAsync(versionedContent);
                if (!db.GetAll<FileContent>().Any(fc => fc.FileId == file.Id))
                    db.DeleteFile(file.Id);
            }
            else
            {
                log.LogWarning($"User can't delete a file in folder {file.Folder.Path.ToLower()} in DMS for app {app.Id}");
                throw new SecurityException("Access Denied!");
            }
        }

        async Task DropFolder(Objects.Path path)
        {
            log.LogDebug("Dropping folder " + path);
            Folder folder = db.GetAll<Folder>()
                .Include(f => f.Roles)
                    .ThenInclude(fr => fr.Role)
                .FirstOrDefault(f => f.AppId == app.Id && f.Path.ToLower() == path.Lowered);

            if (folder != null && folder.UserCan(db.User, "folder_delete"))
                await db.DeleteFolder(folder.Id);
            else
            {
                log.LogWarning($"User can't delete a folder @ path {path.Lowered} in DMS for app {app.Id}");
                throw new SecurityException("Access Denied!");
            }
        }

        async Task<Folder> BuildPath(Objects.Path folderPath)
        {
            if (folderPath.Length > 0)
            {
                Folder existingFolder = db.GetAll<Folder>()
                    .Include(f => f.Roles)
                        .ThenInclude(fr => fr.Role)
                    .FirstOrDefault(f => f.AppId == app.Id && f.Path.ToLower() == folderPath.Lowered);

                if (existingFolder == null)
                    existingFolder = await CreateFolder(folderPath);

                return existingFolder;
            }
            else
                return null;
        }

        async Task<Folder> CreateFolder(Objects.Path folderPath)
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
}
