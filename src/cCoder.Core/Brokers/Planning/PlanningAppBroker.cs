// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Exposures;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Brokers.Planning;

internal class PlanningAppBroker(IWorkflowAppExposure workflowAppExposure) : IPlanningAppBroker
{
    public ValueTask AddAppAsync(App newApp) =>
        workflowAppExposure.AddAsync(newApp: newApp);

    public ValueTask UpdateAppAsync(App updatedApp) =>
        workflowAppExposure.UpdateAsync(updatedApp: updatedApp);

    public ValueTask DeleteAsync(int appId) =>
        workflowAppExposure.DeleteAsync(appId: appId);
}