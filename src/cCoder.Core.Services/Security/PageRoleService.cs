using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Security;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace cCoder.Core.Services.Security
{
    public class PageRoleService : CoreService<PageRole>, ICoreService<PageRole>
    {
        public PageRoleService(ICoreDataContext db) : base(db) { }

        public override Task<PageRole> AddAsync(PageRole entity)
        {
            // make sure we pull back the users for given role
            (Role role, Page page) = GetRoleAndPage(entity);

            // If both Role and User exist but aren't joined
            if (role != null && page != null && page.UserCan(User, "pagerole_create"))
            {
                return !page.Roles.Any(r => r.RoleId == role.Id)
                    ? Db.AddAsync(entity)
                    : Task.FromResult(entity);
            }

            throw new SecurityException("Access Denied!");
        }

        (Role role, Page page) GetRoleAndPage(PageRole entity)
            => (
                Db.GetAll<Role>(false)
                    .FirstOrDefault(r => r.Id == entity.RoleId),
                Db.GetAll<Page>(false)
                    .Include(f => f.Roles)
                        .ThenInclude(fr => fr.Role)
                    .FirstOrDefault(u => u.Id == entity.PageId)
            );

        public override Task DeleteAsync(object id)
        {
            PageRole link = (PageRole)id;

            Page page = Db.GetAll<Page>(true)
                .Include(f => f.Roles)
                    .ThenInclude(fr => fr.Role)
                .FirstOrDefault(u => u.Id == link.PageId);

            PageRole dbVersion = Db.GetAll<PageRole>()
                .Include(ur => ur.Role)
                .FirstOrDefault(ur => ur.RoleId == link.RoleId && ur.PageId == link.PageId);

            return dbVersion != null && page.UserCan(User, "pagerole_delete")
                ? base.DeleteAllAsync(new List<PageRole> { dbVersion })
                : throw new SecurityException("Access Denied!");
        }
    }
}