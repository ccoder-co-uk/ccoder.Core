using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.Mail
{
    /// <summary>
    /// A successfully sent email
    /// </summary>
    [Table("SentEmails", Schema = "Mail")]
    [ApiIgnore]
    public class SentEmail : Email
    {
        public DateTimeOffset SentOn { get; set; }

        [Required]
        public string From { get; set; }
    }
}