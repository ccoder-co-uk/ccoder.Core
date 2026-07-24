// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.AppSecurity;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.AppSecurity;

internal class AppSecurityAppService(IAppSecurityAppBroker appSecurityAppBroker)
    : IAppSecurityAppService
{
    public ValueTask AddAsync(App app) =>
        appSecurityAppBroker.AddAsync(app: app);

    public ValueTask UpdateAsync(App app) =>
        appSecurityAppBroker.UpdateAsync(app: app);

    public ValueTask DeleteAsync(int appId) =>
        appSecurityAppBroker.DeleteAsync(appId: appId);
}