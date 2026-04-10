using cCoder.Scheduling.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Planning;
using cCoder.Data.Models.Security;
using cCoder.Data.Models.Workflow;

namespace cCoder.Core.Brokers.Planning;

public interface IPlanningAppBroker
{
    ValueTask AddAsync(App app);
    ValueTask UpdateAsync(App app);
    ValueTask DeleteAsync(int appId);
}

