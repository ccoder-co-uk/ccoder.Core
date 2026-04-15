using cCoder.Core.Brokers.Mail;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.Mail;

internal class MailAppService(IMailAppBroker mailAppBroker) : IMailAppService
{
    public ValueTask AddAsync(App app) => mailAppBroker.AddAsync(app);
    public ValueTask UpdateAsync(App app) => mailAppBroker.UpdateAsync(app);
    public ValueTask DeleteAsync(int appId) => mailAppBroker.DeleteAsync(appId);
}

