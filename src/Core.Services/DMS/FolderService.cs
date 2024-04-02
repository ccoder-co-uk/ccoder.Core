using Core.Objects;
using Core.Objects.Dtos;
using Core.Objects.Entities.CMS;
using Core.Objects.Entities.DMS;
using Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security;

using File = Core.Objects.Entities.DMS.File;
using Path = Core.Objects.Path;

namespace Core.Services.DMS
{

    public class FolderService : CoreService<Folder>, IFolderService
    {
        private readonly ILogger<FolderService> log;

        public FolderService(ICoreDataContext db, ILogger<FolderService> log) : base(db) =>
            this.log = log;

        public async Task<List<Result<Guid?>>> Copy(string source, string destination, int sourceAppId, int destAppId)
        {
            Folder sourceFolder = Db.GetAll<Folder>().Where(r => r.AppId == sourceAppId)
                .Include(f => f.Files)
                .ThenInclude(f => f.Contents)
                .Include(f => f.Roles)
                .ThenInclude(r => r.Role)
                .ThenInclude(u => u.Users)
                .FirstOrDefault(r => r.Path == source.ToLower());

            Folder destinationFolder = Db.GetAll<Folder>().Where(r => r.AppId == destAppId)
               .Include(f => f.Files)
               .ThenInclude(f => f.Contents)
               .Include(f => f.Roles)
               .ThenInclude(r => r.Role)
               .ThenInclude(u => u.Users)
               .FirstOrDefault(r => r.Path == destination.ToLower());

            if (sourceFolder == null)
                throw new InvalidOperationException("Source folder doesn't exist.");

            if (!sourceFolder.Roles.Any(r => r.Role.Users.Any(u => u.UserId == User.Id) && r.Role.Privileges.Contains("file_update") && r.Role.Privileges.Contains("file_create")) && !User.IsAdminOfApp(destAppId))
                throw new SecurityException("Access Denied!");

            if (destinationFolder == null)
                throw new InvalidOperationException("Destination folder doesn't exist.");

            if (!destinationFolder.Roles.Any(r => r.Role.Users.Any(u => u.UserId == User.Id) && r.Role.Privileges.Contains("file_update") && r.Role.Privileges.Contains("file_create")) && !User.IsAdminOfApp(destAppId))
                throw new SecurityException("Access Denied!");

            File[] sourceFiles = sourceFolder.Files.ToArray();
            var dmsHandleDest = new Core.DMS(Db.Get<App>(destAppId), Db, log);

            List<Result<Guid?>> results = new();

            foreach (File entry in sourceFiles)
            {
                using System.IO.MemoryStream sourceStream = new(entry.Contents.OrderBy(k => k.Version).FirstOrDefault().RawData);

                try
                {
                    await dmsHandleDest.Save(new Path($"{destinationFolder.Path}/{entry.Name}"), sourceStream);
                    results.Add(new Result<Guid?> { Item = entry.Id, Success = true, Id = entry.Id.ToString() });
                }
                catch (Exception ex)
                {
                    results.Add(new Result<Guid?> { Item = null, Success = false, Id = entry.Id.ToString(), Message = ex.Message });
                }
            }

            return results;
        }

        public override async Task<Folder> AddAsync(Folder newFolder)
        {
            if (newFolder.ParentId != null)
            {
                Folder parent = Get(newFolder.ParentId.Value);

                if (parent == null)
                    throw new SecurityException("Access Denied!");

                newFolder.Path = $"{parent.Path}/{newFolder.Name}";
            }
            else
                newFolder.Path = newFolder.Name;

            Folder existingFolder = GetAll().FirstOrDefault(f => f.AppId == newFolder.AppId && f.Path.ToLower() == newFolder.Path.ToLower());

            if (existingFolder != null)
                return existingFolder;
            else
            {
                await new Core.DMS(Db.Get<App>(newFolder.AppId), Db, log)
                    .Save(new Path(newFolder.Path));

                return GetAll().FirstOrDefault(f => f.AppId == newFolder.AppId && f.Path.ToLower() == newFolder.Path.ToLower());
            }
        }

        public override async Task DeleteAsync(object id)
        {
            Folder dbVersion = Db.GetAll<Folder>(false)
               .Include(f => f.App)
               .Include(f => f.Roles)
                   .ThenInclude(fr => fr.Role)
               .FirstOrDefault(f => f.Id == (Guid)id);

            if (dbVersion != null && (dbVersion.App.IsAppAdmin(User) || dbVersion.UserCan(User, "folder_delete")))
                await Db.DeleteFolder((Guid)id);
            else
                throw new SecurityException("Access Denied!");
        }

        public override async Task<Folder> UpdateAsync(Folder folder)
        {
            Folder dbVersion = GetFolderForUpdate(folder);

            if (dbVersion != null && (dbVersion.App.IsAppAdmin(User) || dbVersion.UserCan(User, "folder_update")))
                return await UpdateInternal(dbVersion, folder);

            throw new SecurityException("Access Denied!");
        }

        async Task<Folder> UpdateInternal(Folder dbVersion, Folder folder)
        {
            string parentPath = new Path(folder.Path).ParentPath.FullPath;
            string newPath = $"{(!string.IsNullOrEmpty(parentPath) ? "/" : "")}{folder.Name.ToLower()}";
            Folder existingDestionFolder = GetAll().FirstOrDefault(f => f.Path == newPath && f.Path != dbVersion.Path && f.AppId == folder.AppId);

            // If new parent, go get the new Parent and make sure it exists
            if (folder.ParentId != dbVersion.ParentId)
            {
                Folder newParent = Db.Get<Folder>(folder.ParentId);

                if (newParent != null)
                    dbVersion.Parent = newParent;
            }

            _ = dbVersion.UpdateFrom(folder);
            dbVersion.RecomputePaths();

            //If the folder already exists at the destination then move everything over to that folder (i.e: auto-merge them)
            if (existingDestionFolder != null)
                await MergeSourceIntoDestination(dbVersion, existingDestionFolder);

            await UpdateChildren(folder, existingDestionFolder ?? dbVersion);

            //Since above we moved everything to the existing folder, delete the folder about to be renamed to the new.
            if (existingDestionFolder != null)
                await Db.DeleteFolder(dbVersion.Id);

            //Following logic above... Return the destination folder.
            return existingDestionFolder ?? dbVersion;
        }

        private async Task MergeSourceIntoDestination(Folder dbVersion, Folder existingDestionFolder)
        {
            if (dbVersion.Files != null && dbVersion.Files.Any())
                dbVersion.Files.ForEach(f => f.FolderId = existingDestionFolder.Id);

            if (dbVersion.SubFolders != null && dbVersion.SubFolders.Any())
                dbVersion.SubFolders.ForEach(sf => sf.ParentId = existingDestionFolder.Id);

            _ = await Db.SaveChangesAsync();
        }

        private async Task UpdateChildren(Folder folder, Folder dbVersion)
        {
            if (dbVersion.Files != null && dbVersion.Files.Any())
                dbVersion.Files.ForEach(f => f.RecomputePath());

            if (folder.Roles != null && folder.Roles.Any())
            {
                await Db.DeleteAllAsync(dbVersion.Roles);
                dbVersion.Roles = folder.Roles;
            }

            if (dbVersion.SubFolders != null)
            {
                _ = await Db.SaveChangesAsync();
                foreach (Folder f in dbVersion.SubFolders)
                {
                    f.ParentId = dbVersion.Id;
                    _ = f.Id != Guid.Empty ? await UpdateAsync(f) : await AddAsync(f);
                }
            }
        }

        private Folder GetFolderForUpdate(Folder folder)
            => Db.GetAll<Folder>(true)
                .Include(f => f.App)
                .Include(f => f.SubFolders)
                .Include(f => f.Parent)
                .Include(f => f.Files)
                .Include(f => f.Roles)
                    .ThenInclude(fr => fr.Role)
                .AsSplitQuery()
                .FirstOrDefault(f => f.Id == folder.Id);
    }
}