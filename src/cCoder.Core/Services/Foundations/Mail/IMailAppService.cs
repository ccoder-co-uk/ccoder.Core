using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.Mail;

public interface IMailAppService
{
    ValueTask AddAsync(App app);
    ValueTask UpdateAsync(App app);
    ValueTask DeleteAsync(int appId);
}
