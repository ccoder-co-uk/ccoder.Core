// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;

namespace Workflow.Models;

public sealed class WorkflowConfiguration
{
    public WorkflowConfiguration() =>
        Data = new DataConfiguration();

    public DataConfiguration Data { get; set; }
}