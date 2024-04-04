using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Services;

namespace cCoder.Core.Api.Controllers
{
    public class PrivilegeController : CoreEntityODataController<Privilege, string>
    {
        public PrivilegeController(ICoreService<Privilege> service, ICoreAuthInfo auth, ILogger<PrivilegeController> log) 
            : base(service, auth, log) { }
    }
}