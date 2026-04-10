using cCoder.Core.Brokers.AppSecurity;
using CoreApp = cCoder.Core.Models.App;
using DomainApp = cCoder.Data.Models.CMS.App;

namespace cCoder.Core.Services.Foundations.AppSecurity;

internal class AppSecurityAppService(IAppSecurityAppBroker appSecurityAppBroker) : IAppSecurityAppService
{
    public ValueTask AddAsync(CoreApp app) => appSecurityAppBroker.AddAsync(ToLocalApp(app));
    public ValueTask UpdateAsync(CoreApp app) => appSecurityAppBroker.UpdateAsync(ToLocalApp(app));
    public ValueTask DeleteAsync(int appId) => appSecurityAppBroker.DeleteAsync(appId);

    private static DomainApp ToLocalApp(CoreApp app) =>
        app == null
            ? null
            : new DomainApp
            {
                Id = app.Id,
                DefaultCultureId = app.DefaultCultureId,
                TenantId = app.TenantId,
                Name = app.Name,
                Domain = app.Domain,
                DefaultTheme = app.DefaultTheme,
                ConfigJson = app.ConfigJson,
                Roles = app.Roles?.ToArray(),
            };
}

