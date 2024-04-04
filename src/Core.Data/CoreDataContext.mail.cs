using cCoder.Core.Objects.Entities.Mail;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Data
{
    public partial class CoreDataContext
    {
        // Mail
        public virtual DbSet<MailServer> MailServers { get; set; }
        public virtual DbSet<QueuedEmail> QueuedMail { get; set; }
        public virtual DbSet<SentEmail> SentMail { get; set; }
        public virtual DbSet<EmailSendFailure> SendFailures { get; set; }
    }
}
