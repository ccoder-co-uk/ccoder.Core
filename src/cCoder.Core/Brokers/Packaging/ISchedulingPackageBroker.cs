// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;
using cCoder.Workflow.Models;

namespace cCoder.Core.Brokers.Packaging;

internal interface ISchedulingPackageBroker
{
    ValueTask ImportPackageAsync(int appId, WorkflowPackage workflowPackage);
    WorkflowPackage ExportPackage(int appId, string packageName);
}