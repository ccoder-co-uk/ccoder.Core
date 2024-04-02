using Core.Objects;
using Core.Objects.Entities.Workflow;
using Core.Services;

namespace Core.Api.Controllers
{
    public class WorkflowEventController : CoreEntityODataController<WorkflowEvent, Guid>
    {
        public WorkflowEventController(ICoreService<WorkflowEvent> service, ICoreAuthInfo auth, ILogger<WorkflowEventController> log) 
            : base(service, auth, log) { }
    }
}