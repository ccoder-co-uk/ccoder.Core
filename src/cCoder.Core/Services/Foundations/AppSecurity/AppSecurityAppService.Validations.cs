// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;
using cCoder.Core.Dependencies;

namespace cCoder.Core.Services.Foundations.AppSecurity;

internal sealed partial class AppSecurityAppService
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