using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.CMS;

[Table("Layouts", Schema = "CMS")]
public class Layout : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("App")]
    public int AppId { get; set; }

    public string HeaderHtml { get; set; }
    public string Html { get; set; }
    public string Script { get; set; }
    public virtual App App { get; set; }
}