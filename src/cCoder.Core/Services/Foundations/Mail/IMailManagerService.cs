using cCoder.Mail.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Security;

namespace cCoder.Core.Services.Foundations.Mail;

public interface IMailManagerService
{
    ValueTask<QueuedEmail> AddAsync(QueuedEmail email, bool checkPrivileges = false);
}

