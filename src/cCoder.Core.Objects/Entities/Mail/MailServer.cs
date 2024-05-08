using cCoder.Core.Objects.Entities.CMS;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.Mail;

[Table("MailServers", Schema = "Mail")]
public class MailServer
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("App")]
    public int AppId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string User { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string Host { get; set; }

    public string FromEmail { get; set; }

    public int Port { get; set; }

    public bool EnableSSL { get; set; }

    public virtual App App { get; set; }
}