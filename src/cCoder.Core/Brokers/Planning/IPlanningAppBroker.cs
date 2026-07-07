using cCoder.Data.Models.CMS;

namespace cCoder.Core.Brokers.Planning;

public interface IPlanningAppBroker
{
    ValueTask AddAsync(App app);
    ValueTask UpdateAsync(App app);
    ValueTask DeleteAsync(int appId);
}

