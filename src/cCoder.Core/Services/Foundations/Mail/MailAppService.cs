using cCoder.Core.Brokers.Mail;
using cCoder.Core.Models;

namespace cCoder.Core.Services.Foundations.Mail;

internal class MailAppService(IMailAppBroker mailAppBroker) : IMailAppService
{
    public ValueTask AddAsync(App app) => mailAppBroker.AddAsync(ToLocalApp(app));
    public ValueTask UpdateAsync(App app) => mailAppBroker.UpdateAsync(ToLocalApp(app));
    public ValueTask DeleteAsync(int appId) => mailAppBroker.DeleteAsync(appId);

    private static cCoder.Data.Models.CMS.App ToLocalApp(App app) =>
        app == null
            ? null
            : new cCoder.Data.Models.CMS.App
            {
                Id = app.Id,
                Name = app.Name,
                MailServers = app.MailServers?.ToArray(),
                MailQueue = app.MailQueue?.ToArray(),
                SentMail = app.SentMail?.ToArray(),
            };
}

