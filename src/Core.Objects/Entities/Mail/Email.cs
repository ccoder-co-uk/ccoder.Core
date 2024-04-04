using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Security;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.Mail
{
    public class Email
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("App")]
        public int AppId { get; set; }

        [ForeignKey("SentBy")]
        public string SentByUserId { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string To { get; set; }

        public string CC { get; set; }

        public bool IsBodyHtml { get; set; } = true;

        public virtual App App { get; set; }

        public virtual User SentBy { get; set; }
    }
}
