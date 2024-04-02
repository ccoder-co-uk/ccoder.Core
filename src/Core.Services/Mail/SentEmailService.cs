using Core.Objects;
using Core.Objects.Entities.Mail;

namespace Core.Services.CMS
{
    public class SentEmailService : CoreService<SentEmail>, ICoreService<SentEmail>
    {
        public SentEmailService(ICoreDataContext db) : base(db) { }
    }
}