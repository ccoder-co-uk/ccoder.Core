// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Core.Models;

namespace cCoder.Core.Services.Orchestrations;

internal sealed partial class TemplatedEmailOperationOrchestrationService
{
    private static void ValidateTemplatedEmailOperationOnQueue(
        TemplatedEmailOperation templatedEmailOperation) =>
        ValidationRulesEngine.Validate(inputs: [templatedEmailOperation]);
}