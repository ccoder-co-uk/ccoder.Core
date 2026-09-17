// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Core.Models;
using TemplatedEmailDetails = cCoder.Mail.Models.TemplatedEmailDetails;

namespace cCoder.Core.Services.Orchestrations;

internal sealed partial class TemplatedEmailOrchestrationService
{
    private static void ValidateTemplatedEmailOperationOnQueue(
        TemplatedEmailOperation templatedEmailOperation) =>
        ValidationRulesEngine.Validate(inputs: [templatedEmailOperation]);

    private static void ValidateTemplatedEmailDetailsOnQueue(
        TemplatedEmailDetails templatedEmailDetails) =>
        ValidationRulesEngine.Validate(inputs: [templatedEmailDetails]);
}