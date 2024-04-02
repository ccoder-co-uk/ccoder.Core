using Core.Objects;
using Core.Objects.Entities.Workflow;
using Core.Services;

namespace Core.Api.Controllers
{
    public class BusinessProcessController : CoreEntityODataController<BusinessProcess, Guid>
    {
        public BusinessProcessController(ICoreService<BusinessProcess> service, ICoreAuthInfo auth, ILogger<BusinessProcessController> log) 
            : base(service, auth, log) { }
    }
}
