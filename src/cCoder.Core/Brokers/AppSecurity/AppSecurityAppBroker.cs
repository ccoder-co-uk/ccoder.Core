// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;
using cCoder.AppSecurity.Exposures;
using cCoder.Data.Models.CMS;

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