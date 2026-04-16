using cCoder.Core.Brokers.Workflow;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.Workflow;

internal class WorkflowAppService(IWorkflowAppBroker workflowAppBroker) : IWorkflowAppService
{
    public ValueTask AddAsync(App app) => workflowAppBroker.AddAsync(app);
    public ValueTask UpdateAsync(App app) => workflowAppBroker.UpdateAsync(app);
    public ValueTask DeleteAsync(int appId) => workflowAppBroker.DeleteAsync(appId);
}

