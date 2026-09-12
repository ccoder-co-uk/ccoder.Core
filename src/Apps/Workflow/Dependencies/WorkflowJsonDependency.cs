// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Activities.Models;
using cCoder.Workflow.Engine.Extensions;
using Newtonsoft.Json;

namespace Workflow.Dependencies;

internal sealed class WorkflowJsonDependency
{
    public WorkflowRequest DeserializeWorkflowRequest(string json) =>
        JsonConvert.DeserializeObject<WorkflowRequest>(
            value: json,
            settings: ObjectExtensions.GetJsonSettings());
}