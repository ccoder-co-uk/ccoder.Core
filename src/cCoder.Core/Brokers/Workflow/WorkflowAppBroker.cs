// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Exposures;
using cCoder.Workflow.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Security;
using cCoder.Data.Models.Workflow;

namespace cCoder.Core.Brokers.Workflow;

internal class WorkflowAppBroker(IWorkflowAppExposure workflowAppExposure) : IWorkflowAppBroker
{
    public ValueTask AddAsync(App app) =>
        workflowAppExposure.AddAsync(newApp: app);

    public ValueTask UpdateAsync(App app) =>
        workflowAppExposure.UpdateAsync(updatedApp: app);

    public ValueTask DeleteAsync(int appId) =>
        workflowAppExposure.DeleteAsync(appId: appId);
}