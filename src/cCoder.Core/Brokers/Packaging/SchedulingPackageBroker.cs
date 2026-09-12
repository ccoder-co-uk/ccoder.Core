// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Exposures;
using cCoder.Workflow.Models;

namespace cCoder.Core.Brokers.Packaging;

internal sealed class SchedulingPackageBroker(
    IWorkflowPackageManager workflowPackageManager)
    : ISchedulingPackageBroker
{
    public ValueTask ImportPackageAsync(int appId, WorkflowPackage workflowPackage) =>
        workflowPackageManager.ImportPackageAsync(
            appId: appId,
            workflowPackage: workflowPackage);

    public WorkflowPackage ExportPackage(int appId, string packageName) =>
        workflowPackageManager.ExportPackage(appId: appId, packageName: packageName);
}