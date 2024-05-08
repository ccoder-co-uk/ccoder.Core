using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Mail;

namespace cCoder.Core.Services.Mail;

public class MailServerService : CoreService<MailServer>, ICoreService<MailServer>
{
    public MailServerService(ICoreDataContext db) : base(db) { }
}