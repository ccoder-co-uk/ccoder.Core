using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.AppSecurity;

public interface IAppSecurityAppService
{
    ValueTask AddAsync(App app);
    ValueTask UpdateAsync(App app);
    ValueTask DeleteAsync(int appId);
}