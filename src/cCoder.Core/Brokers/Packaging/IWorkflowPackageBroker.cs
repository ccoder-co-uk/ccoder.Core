// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Models;

namespace cCoder.Core.Brokers.Packaging;

internal interface IWorkflowPackageBroker
{
    ValueTask ImportPackageAsync(int appId, WorkflowPackage workflowPackage);
    WorkflowPackage ExportPackage(int appId, string packageName);
}