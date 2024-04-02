using Core.Objects;
using Core.Objects.Entities.Security;
using Core.Services;

namespace Core.Api.Controllers
{
    public class PrivilegeController : CoreEntityODataController<Privilege, string>
    {
        public PrivilegeController(ICoreService<Privilege> service, ICoreAuthInfo auth, ILogger<PrivilegeController> log) 
            : base(service, auth, log) { }
    }
}