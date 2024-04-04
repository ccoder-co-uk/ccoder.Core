using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Objects.Entities.Security;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace cCoder.Core.Services.DMS
{
    public class FolderRoleService : CoreService<FolderRole>, ICoreService<FolderRole>
    {
        public FolderRoleService(ICoreDataContext db) : base(db) { }

        public override Task<FolderRole> AddAsync(FolderRole entity)
        {
            (Role role, Folder folder) = GetFolderAndRole(entity);

            // If both Role and User exist but aren't joined
            bool roleAndFolderExist = role != null && folder != null;
            var UserHasPermission = (Folder folder, Role role) => folder.UserCan(User, "folderrole_create");

            if (roleAndFolderExist && UserHasPermission(folder, role))
                return !folder.Roles.Any(r => r.RoleId == role.Id) ? Db.AddAsync(entity) : Task.FromResult(entity);
            else if (role != null && folder != null && role.Folders.Any(r => r.FolderId == folder.Id))
                return Task.FromResult(entity);

            throw new SecurityException("Access Denied!");
        }

        (Role role, Folder folder) GetFolderAndRole(FolderRole entity)
         => (
            Db.GetAll<Role>(false).FirstOrDefault(r => r.Id == entity.RoleId),
            Db.GetAll<Folder>(false)
                .Include(f => f.Roles)
                    .ThenInclude(fr => fr.Role)
                .FirstOrDefault(u => u.Id == entity.FolderId)
            );

        public override Task DeleteAsync(object id)
        {
            FolderRole link = (FolderRole)id;

            Folder folder = Db.GetAll<Folder>(true)
                .Include(f => f.Roles)
                    .ThenInclude(fr => fr.Role)
                .FirstOrDefault(u => u.Id == link.FolderId);

            FolderRole dbVersion = Db.GetAll<FolderRole>()
                .Include(ur => ur.Role)
                .FirstOrDefault(ur => ur.RoleId == link.RoleId && ur.FolderId == link.FolderId);

            return dbVersion != null && folder.UserCan(User, "folderrole_delete")
                ? base.DeleteAllAsync(new List<FolderRole> { dbVersion })
                : throw new SecurityException("Access Denied!");
        }
    }
}