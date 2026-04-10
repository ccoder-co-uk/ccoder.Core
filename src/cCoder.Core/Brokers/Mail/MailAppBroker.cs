using cCoder.Mail.Exposures;
using cCoder.Mail.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Security;

namespace cCoder.Core.Brokers.Mail;

internal class MailAppBroker(IMailAppExposure mailAppExposure) : IMailAppBroker
{
    public ValueTask AddAsync(App app) => mailAppExposure.AddAsync(app);
    public ValueTask UpdateAsync(App app) => mailAppExposure.UpdateAsync(app);
    public ValueTask DeleteAsync(int appId) => mailAppExposure.DeleteAsync(appId);
}

