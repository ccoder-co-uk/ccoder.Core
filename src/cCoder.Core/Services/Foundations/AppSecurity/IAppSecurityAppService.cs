// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.AppSecurity;

internal interface IAppSecurityAppService
{
    ValueTask AddAppAsync(App newApp);

    ValueTask UpdateAppAsync(App updatedApp);

    ValueTask DeleteAppAsync(int appId);
}