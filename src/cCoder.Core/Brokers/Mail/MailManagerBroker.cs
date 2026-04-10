using cCoder.Mail.Exposures;
using cCoder.Mail.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Security;

namespace cCoder.Core.Brokers.Mail;

internal class MailManagerBroker(IMailManagerExposure mailManagerExposure) : IMailManagerBroker
{
    public ValueTask<QueuedEmail> AddAsync(QueuedEmail email, bool checkPrivileges = false) =>
        mailManagerExposure.AddAsync(email, checkPrivileges);
}

