using Core.Objects;
using Core.Objects.Entities.Security;
using Core.Services;

namespace Core.Api.Controllers
{
    public class RoleController : CoreEntityODataController<Role, Guid>
    {
        public new ICoreService<Role> Service => 
            base.Service as ICoreService<Role>;

        public RoleController(ICoreService<Role> service, ICoreAuthInfo auth, ILogger<RoleController> log) 
            : base(service, auth, log) { }
    }
}