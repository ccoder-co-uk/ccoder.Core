using cCoder.Core.Brokers.Mail;
using cCoder.Mail.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Security;

namespace cCoder.Core.Services.Foundations.Mail;

internal class MailManagerService(IMailManagerBroker mailManagerBroker) : IMailManagerService
{
    public ValueTask<QueuedEmail> AddAsync(QueuedEmail email, bool checkPrivileges = false) =>
        mailManagerBroker.AddAsync(email, checkPrivileges);
}

