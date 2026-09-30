// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;
using cCoder.Workflow.Exposures;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Brokers.Planning;

internal class PlanningAppBroker(IWorkflowAppExposure workflowAppExposure) : IPlanningAppBroker
{
    public ValueTask AddAppAsync(App newApp) =>
        workflowAppExposure.AddAsync(newApp: newApp);

    public ValueTask UpdateAppAsync(App updatedApp) =>
        workflowAppExposure.UpdateAsync(updatedApp: updatedApp);

    public ValueTask DeleteAppAsync(int appId) =>
        workflowAppExposure.DeleteAsync(appId: appId);
}