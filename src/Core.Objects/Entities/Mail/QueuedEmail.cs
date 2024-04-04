using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.Mail
{
    /// <summary>
    /// An email that lives on the sending queue
    /// </summary>
    [Table("QueuedEmails", Schema = "Mail")]
    [ApiIgnore]
    public class QueuedEmail : Email
    {
        public virtual ICollection<EmailSendFailure> FailedSends { get; set; }

        [Required]
        public string MailServerName { get; set; }
    }
}