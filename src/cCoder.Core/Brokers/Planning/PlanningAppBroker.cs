// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Exposures;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Brokers.Planning;

internal class PlanningAppBroker(IWorkflowAppExposure workflowAppExposure) : IPlanningAppBroker
{
    public ValueTask AddAsync(App app) =>
        workflowAppExposure.AddAsync(newApp: app);

    public ValueTask UpdateAsync(App app) =>
        workflowAppExposure.UpdateAsync(updatedApp: app);

    public ValueTask DeleteAsync(int appId) =>
        workflowAppExposure.DeleteAsync(appId: appId);
}