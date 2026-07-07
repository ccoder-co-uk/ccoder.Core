using cCoder.Workflow.Exposures;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Brokers.Planning;

internal class PlanningAppBroker(IWorkflowAppExposure workflowAppExposure) : IPlanningAppBroker
{
    public ValueTask AddAsync(App app) => workflowAppExposure.AddAsync(app);
    public ValueTask UpdateAsync(App app) => workflowAppExposure.UpdateAsync(app);
    public ValueTask DeleteAsync(int appId) => workflowAppExposure.DeleteAsync(appId);
}

