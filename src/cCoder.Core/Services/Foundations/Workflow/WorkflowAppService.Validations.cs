// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.Workflow;

internal sealed partial class WorkflowAppService
{
    private static void Validate(params object[] inputs) =>
        ValidationRulesEngine.Validate(inputs: inputs);

    private static void ValidateAppOnAdd(App newApp) =>
        Validate(inputs: [newApp]);

    private static void ValidateAppOnUpdate(App updatedApp) =>
        Validate(inputs: [updatedApp]);

    private static void ValidateAppOnDelete(int appId) =>
        Validate(inputs: [appId]);
}