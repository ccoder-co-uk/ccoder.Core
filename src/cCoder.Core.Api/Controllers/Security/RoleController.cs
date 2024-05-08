using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Services;

namespace cCoder.Core.Api.Controllers.Security;

public class RoleController : CoreEntityODataController<Role, Guid>
{
    public new ICoreService<Role> Service =>
        base.Service as ICoreService<Role>;

    public RoleController(ICoreService<Role> service, ICoreAuthInfo auth, ILogger<RoleController> log)
        : base(service, auth, log) { }
}