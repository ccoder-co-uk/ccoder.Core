// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Core.Models;

namespace cCoder.Core.Services.Foundations.TemplatedEmails;

internal sealed partial class TemplatedEmailContentService
{
    private static void ValidateTemplatedEmailOperationOnResolve(
        TemplatedEmailOperation templatedEmailOperation) =>
        ValidationRulesEngine.Validate(inputs: [templatedEmailOperation]);

    private static void ValidateTemplatedEmailOperationOnRender(
        TemplatedEmailOperation templatedEmailOperation) =>
        ValidationRulesEngine.Validate(inputs: [templatedEmailOperation]);
}