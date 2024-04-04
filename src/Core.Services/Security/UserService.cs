using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Security;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace cCoder.Core.Services.Security
{
    public class UserService : CoreService<User>, ICoreService<User>
    {
        public UserService(ICoreDataContext db) : base(db) { }

        public override async Task<User> AddAsync(User newUser)
        {
            var existingUser = Db.GetAll<User>(false)
                .IgnoreQueryFilters()
                .FirstOrDefault(u => u.Id == newUser.Id || u.Email == newUser.Email);

            return existingUser != null
                ? existingUser
                : await Db.AddAsync(newUser);
        }

        public override Task DeleteAsync(object id)
        {
            User dbVersion = Get(id);
            return AuthInfo.SSOUserId == dbVersion.Id ? base.DeleteAsync(id) : throw new SecurityException("Access Denied!");
        }

        public override Task<User> UpdateAsync(User entity)
            => AuthInfo.SSOUserId == entity.Id ? Db.UpdateAsync(entity) : throw new SecurityException("Access Denied!");
    }
}