// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.AppSecurity;

internal sealed partial class AppSecurityUserRoleService
{
    private static void ValidateAppOnSave(App app) =>
        ArgumentNullException.ThrowIfNull(
            argument: app);
}