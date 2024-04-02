using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Objects.Entities.Mail
{
    [Table("EmailSendFailures", Schema = "Mail")]
    [Parent("Email")]
    public class EmailSendFailure
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Email")]
        public int EmailId { get; set; }

        public DateTimeOffset AttemptedOn { get; set; }

        [Required]
        public string FailureReason { get; set; }

        public virtual QueuedEmail Email { get; set; }
    }
}