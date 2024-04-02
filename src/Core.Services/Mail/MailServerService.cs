using Core.Objects;
using Core.Objects.Entities.Mail;

namespace Core.Services.CMS
{
    public class MailServerService : CoreService<MailServer>, ICoreService<MailServer>
    {
        public MailServerService(ICoreDataContext db) : base(db) { }
    }
}