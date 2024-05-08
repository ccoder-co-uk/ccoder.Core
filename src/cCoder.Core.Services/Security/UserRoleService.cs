using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Security;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security;

namespace cCoder.Core.Services.Security;

public class UserRoleService : CoreService<UserRole>, IUserRoleService
{
    public UserRoleService(ICoreDataContext db) : base(db) { }

    public override async Task<UserRole> AddAsync(UserRole entity)
    {
        Role role = Db.GetAll<Role>(false)
            .Include(r => r.Users)
            .FirstOrDefault(r => r.Id == entity.RoleId);

        User user = Db.GetAll<User>(false)
            .FirstOrDefault(u => u.Id == entity.UserId);

        bool accessGranted = User.Can(role.AppId, "userrole_create") && UserIsActive && !role.Users.Any(u => u.UserId == user.Id);

        return accessGranted
            ? await base.AddAsync(entity)
            : throw new SecurityException("Access Denied!");
    }

    public async Task<UserRole> SaveAsync(UserRole entity)
    {
        Role role = Db.GetAll<Role>(false)
            .IgnoreQueryFilters()
            .Include(r => r.Users)
            .FirstOrDefault(r => r.Id == entity.RoleId);

        if (role is null)
            throw new InvalidOperationException("Role could not be found.");

        User user = Db.GetAll<User>(false)
            .IgnoreQueryFilters()
            .FirstOrDefault(u => u.Id == entity.UserId);

        if (user is null)
            throw new InvalidOperationException("User could not be found.");

        UserRole existingUserRole = role.Users
            .FirstOrDefault(u => u.UserId == user.Id);

        if (existingUserRole != null)
            return existingUserRole;
        
        return await Db.AddAsync(entity);
    }

    public override async Task DeleteAsync(object id)
    {
        UserRole link = (UserRole)id;

        UserRole dbVersion = Db.GetAll<UserRole>()
            .Include(ur => ur.Role)
            .FirstOrDefault(ur => ur.RoleId == link.RoleId && ur.UserId == link.UserId);

        _ = dbVersion != null && User.Can(dbVersion.Role.AppId, "userrole_delete") && UserIsActive
            ? await Db.DeleteAsync(dbVersion)
            : throw new SecurityException("Access Denied!");
    }

    public override async Task DeleteAllAsync(IEnumerable<UserRole> items)
    {
        bool userCan = true;

        // Get all from DB in one go before testing UserCan on each
        IQueryable<UserRole> dbVersions = Db.GetAll<UserRole>().Where(ur => items.Any(i => i.UserId == ur.UserId && i.RoleId == ur.RoleId));

        // If UserCan fails for any, fail for all
        foreach (UserRole dbVersion in dbVersions)
        {
            if (!User.Can(dbVersion.Role.AppId, "userrole_delete"))
            {
                userCan = false;
            }
        }

        if (userCan && UserIsActive)
        {
            await Db.DeleteAllAsync(dbVersions);
        }
        else
        {
            throw new SecurityException("Access Denied!");
        }
    }
}