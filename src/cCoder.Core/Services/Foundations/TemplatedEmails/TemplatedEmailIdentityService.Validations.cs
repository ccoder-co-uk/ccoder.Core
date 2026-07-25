// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Core.Models;

namespace cCoder.Core.Services.Foundations.TemplatedEmails;

internal sealed partial class TemplatedEmailIdentityService
{
    private static void ValidateTemplatedEmailOperationOnResolve(
        TemplatedEmailOperation templatedEmailOperation) =>
        ValidationRulesEngine.Validate(inputs: [templatedEmailOperation]);
}