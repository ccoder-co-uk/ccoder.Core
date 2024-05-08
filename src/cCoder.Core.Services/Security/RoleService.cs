using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Security;

namespace cCoder.Core.Services.Security;

public class RoleService : CoreService<Role>, ICoreService<Role>
{
    public RoleService(ICoreDataContext db) : base(db) { }
}