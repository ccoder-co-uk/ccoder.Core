using cCoder.Core.Brokers.Workflow;
using cCoder.Core.Models;

namespace cCoder.Core.Services.Foundations.Workflow;

internal class WorkflowAppService(IWorkflowAppBroker workflowAppBroker) : IWorkflowAppService
{
    public ValueTask AddAsync(App app) => workflowAppBroker.AddAsync(ToLocalApp(app));
    public ValueTask UpdateAsync(App app) => workflowAppBroker.UpdateAsync(ToLocalApp(app));
    public ValueTask DeleteAsync(int appId) => workflowAppBroker.DeleteAsync(appId);

    private static cCoder.Data.Models.CMS.App ToLocalApp(App app) =>
        app == null
            ? null
            : new cCoder.Data.Models.CMS.App
            {
                Id = app.Id,
                Name = app.Name,
                Domain = app.Domain,
                Flows = app.Flows?.ToArray(),
            };
}

