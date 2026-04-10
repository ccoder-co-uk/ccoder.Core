using cCoder.AppSecurity.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Security;

namespace cCoder.Core.Brokers.AppSecurity;

public interface IAppSecurityAppBroker
{
    ValueTask AddAsync(App app);
    ValueTask UpdateAsync(App app);
    ValueTask DeleteAsync(int appId);
}

