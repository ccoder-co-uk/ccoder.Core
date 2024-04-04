using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Services;

namespace cCoder.Core.Api.Controllers
{
    public class WorkflowEventController : CoreEntityODataController<WorkflowEvent, Guid>
    {
        public WorkflowEventController(ICoreService<WorkflowEvent> service, ICoreAuthInfo auth, ILogger<WorkflowEventController> log) 
            : base(service, auth, log) { }
    }
}