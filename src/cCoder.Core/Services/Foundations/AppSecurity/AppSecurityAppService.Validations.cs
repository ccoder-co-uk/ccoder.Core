// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;
using cCoder.Core.Dependencies;

namespace cCoder.Core.Services.Foundations.AppSecurity;

internal sealed partial class AppSecurityAppService
{
    private static void ValidateAppOnAdd(App newApp) =>
        ValidationRulesEngine.Validate(inputs: [newApp]);

    private static void ValidateAppOnUpdate(App updatedApp) =>
        ValidationRulesEngine.Validate(inputs: [updatedApp]);

    private static void ValidateAppOnDelete(int appId) =>
        ValidationRulesEngine.Validate(inputs: [appId]);
}