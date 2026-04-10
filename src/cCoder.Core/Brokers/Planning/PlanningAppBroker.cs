using cCoder.Scheduling.Exposures;
using cCoder.Scheduling.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Planning;
using cCoder.Data.Models.Security;
using cCoder.Data.Models.Workflow;

namespace cCoder.Core.Brokers.Planning;

internal class PlanningAppBroker(ISchedulingAppExposure schedulingAppExposure) : IPlanningAppBroker
{
    public ValueTask AddAsync(App app) => schedulingAppExposure.AddAsync(app);
    public ValueTask UpdateAsync(App app) => schedulingAppExposure.UpdateAsync(app);
    public ValueTask DeleteAsync(int appId) => schedulingAppExposure.DeleteAsync(appId);
}

