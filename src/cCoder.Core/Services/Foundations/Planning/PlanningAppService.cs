using cCoder.Core.Brokers.Planning;
using cCoder.Core.Models;

namespace cCoder.Core.Services.Foundations.Planning;

internal class PlanningAppService(IPlanningAppBroker planningAppBroker) : IPlanningAppService
{
    public ValueTask AddAsync(App app) => planningAppBroker.AddAsync(ToLocalApp(app));
    public ValueTask UpdateAsync(App app) => planningAppBroker.UpdateAsync(ToLocalApp(app));
    public ValueTask DeleteAsync(int appId) => planningAppBroker.DeleteAsync(appId);

    private static cCoder.Data.Models.CMS.App ToLocalApp(App app) =>
        app == null
            ? null
            : new cCoder.Data.Models.CMS.App
            {
                Id = app.Id,
                Name = app.Name,
                Calendars = app.Calendars?.ToArray(),
                Tasks = app.Tasks?.ToArray(),
            };
}

