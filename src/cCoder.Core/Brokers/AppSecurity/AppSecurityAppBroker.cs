// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity.Exposures;
using cCoder.AppSecurity.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Security;

namespace cCoder.Core.Brokers.AppSecurity;

internal class AppSecurityAppBroker(IAppSecurityAppExposure appSecurityAppExposure)
    : IAppSecurityAppBroker
{
    public ValueTask AddAppAsync(App newApp) =>
        appSecurityAppExposure.AddAsync(app: newApp);

    public ValueTask UpdateAppAsync(App updatedApp) =>
        appSecurityAppExposure.UpdateAsync(app: updatedApp);

    public ValueTask DeleteAppAsync(int appId) =>
        appSecurityAppExposure.DeleteAsync(appId: appId);
}