using cCoder.AppSecurity.Exposures;
using cCoder.AppSecurity.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Security;

namespace cCoder.Core.Brokers.AppSecurity;

internal class AppSecurityAppBroker(IAppSecurityAppExposure appSecurityAppExposure)
    : IAppSecurityAppBroker
{
    public ValueTask AddAsync(App app) => appSecurityAppExposure.AddAsync(app);
    public ValueTask UpdateAsync(App app) => appSecurityAppExposure.UpdateAsync(app);
    public ValueTask DeleteAsync(int appId) => appSecurityAppExposure.DeleteAsync(appId);
}

