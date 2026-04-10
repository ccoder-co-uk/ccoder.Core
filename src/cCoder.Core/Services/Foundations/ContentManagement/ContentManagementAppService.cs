using cCoder.Core.Brokers.ContentManagement;
using cCoder.Core.Models;
using DomainApp = cCoder.Data.Models.CMS.App;

namespace cCoder.Core.Services.Foundations.ContentManagement;

internal class ContentManagementAppService(IContentManagementAppBroker contentManagementAppBroker)
    : IContentManagementAppService
{
    public App Get(int id, bool ignoreFilters = false) =>
        ToLocalApp(contentManagementAppBroker.Get(id, ignoreFilters));

    public App GetByDomain(string domain, bool ignoreFilters = false) =>
        ToLocalApp(contentManagementAppBroker.GetByDomain(domain, ignoreFilters));

    public async ValueTask<App> AddAsync(App app) =>
        ToLocalApp(await contentManagementAppBroker.AddAsync(ToExternalApp(app)));

    public async ValueTask<App> UpdateAsync(App app) =>
        ToLocalApp(await contentManagementAppBroker.UpdateAsync(ToExternalApp(app)));

    public ValueTask DeleteAsync(int appId) => contentManagementAppBroker.DeleteAsync(appId);

    private static DomainApp ToExternalApp(App app) =>
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
                ConfigJson = app.ConfigJson
            };

    private static App ToLocalApp(DomainApp app) =>
        app == null
            ? null
            : new App
            {
                Id = app.Id,
                DefaultCultureId = app.DefaultCultureId,
                TenantId = app.TenantId,
                Name = app.Name,
                Domain = app.Domain,
                DefaultTheme = app.DefaultTheme,
                ConfigJson = app.ConfigJson
            };
}

