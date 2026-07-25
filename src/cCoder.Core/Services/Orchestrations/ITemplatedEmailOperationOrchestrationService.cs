// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;

namespace cCoder.Core.Services.Orchestrations;

internal interface ITemplatedEmailOperationOrchestrationService
{
    ValueTask<TemplatedEmailOperation> QueueTemplatedEmailOperationAsync(
        TemplatedEmailOperation templatedEmailOperation);
}