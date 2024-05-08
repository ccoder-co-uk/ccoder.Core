using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Entities.Workflow;
using System.Security;

namespace cCoder.Core.Services.Workflow;

public class WorkflowEventService : CoreService<WorkflowEvent>
{
    public WorkflowEventService(ICoreDataContext db) : base(db) { }

    public override Task<WorkflowEvent> AddAsync(WorkflowEvent entity)
        => SecurityCheckEvent(entity) ? base.AddAsync(entity) : throw new SecurityException("Access Denied!");

    public override Task<WorkflowEvent> UpdateAsync(WorkflowEvent entity)
        => SecurityCheckEvent(entity) ? Db.UpdateAsync(entity) : throw new SecurityException("Access Denied!");

    // checks to confirm task isn't bleeding beyond app scope and that an app admin is the one setting this up
    private bool SecurityCheckEvent(WorkflowEvent workflowEvent)
    {
        FlowDefinition flow = Db.GetAll<FlowDefinition>(false).FirstOrDefault(f => f.Id == workflowEvent.FlowId);
        if (flow == null)
            throw new SecurityException("Access Denied!");

        bool userIsAppAdmin = User.IsAdminOfApp(flow.AppId);
        bool userOnEventIsAppUser = Db.GetAll<User>(false).Any(u => u.Id == workflowEvent.ExecuteAs && u.Roles.Any(r => r.Role.AppId == flow.AppId));
        return userIsAppAdmin && userOnEventIsAppUser;
    }
}
