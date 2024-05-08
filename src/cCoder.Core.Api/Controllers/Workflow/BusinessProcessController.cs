using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Services;

namespace cCoder.Core.Api.Controllers.Workflow;

public class BusinessProcessController : CoreEntityODataController<BusinessProcess, Guid>
{
    public BusinessProcessController(ICoreService<BusinessProcess> service, ICoreAuthInfo auth, ILogger<BusinessProcessController> log)
        : base(service, auth, log) { }
}
