using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Mail;

namespace cCoder.Core.Services.CMS
{
    public class SentEmailService : CoreService<SentEmail>, ICoreService<SentEmail>
    {
        public SentEmailService(ICoreDataContext db) : base(db) { }
    }
}