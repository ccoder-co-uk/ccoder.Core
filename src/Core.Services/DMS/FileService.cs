using Core.Objects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Core.Services.DMS
{
    public class FileService : CoreService<Objects.Entities.DMS.File>, IFileService
    {
        public FileService(ICoreDataContext db) : base(db) { }

        public override Task<Objects.Entities.DMS.File> AddAsync(Objects.Entities.DMS.File newFile) => throw new InvalidOperationException("To create a file, please post to /API/DMS/{path}");

        public Objects.Entities.DMS.File GetByPath(int appId, string path)
        {
            string[] pathParts = path.Split("/".ToCharArray());
            string folderPath = string.Join("/", pathParts.Except(new[] { pathParts.Last() }));
            Objects.Entities.DMS.File result = Db.GetAll<Objects.Entities.DMS.File>(true).Include(f => f.Folder).FirstOrDefault(f => f.Folder.Path.ToLower() == folderPath.ToLower() && f.Folder.AppId == appId && f.Path.ToLower() == path.ToLower());

            return result ?? throw new SecurityException("Access Denied!");
        }

        public override Task DeleteAsync(object id) => throw new InvalidOperationException("To delete a file, please http delete to /API/DMS/{path}");

        public override async Task<Objects.Entities.DMS.File> UpdateAsync(Objects.Entities.DMS.File newFile)
        {
            if (newFile.Contents != null)
            {
                throw new InvalidOperationException("To update file contents, please post to /API/DMS/{path}");
            }

            Objects.Entities.DMS.File dbVersion = Db.GetAll<Objects.Entities.DMS.File>(true)
                .Include(f => f.Folder)
                    .ThenInclude(f => f.Roles)
                        .ThenInclude(fr => fr.Role)
                .FirstOrDefault(f => f.Id == newFile.Id);

            if (dbVersion != null && dbVersion.UserCan(User, "file_update"))
            {
                dbVersion.Description = newFile.Description;

                if (dbVersion.Name != newFile.Name || dbVersion.FolderId != newFile.FolderId)
                {
                    dbVersion.Name = newFile.Name;
                    dbVersion.FolderId = newFile.FolderId;
                    dbVersion.RecomputePath();
                }

                return await base.UpdateAsync(dbVersion);
            }
            else
            {
                throw new SecurityException("Access Denied!");
            }
        }
    }
}