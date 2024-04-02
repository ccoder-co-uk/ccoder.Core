using Core.Objects;
using Core.Objects.Entities.Security;

namespace Core.Services.Security
{
    public class RoleService : CoreService<Role>, ICoreService<Role>
    {
        public RoleService(ICoreDataContext db) : base(db) { }
    }
}