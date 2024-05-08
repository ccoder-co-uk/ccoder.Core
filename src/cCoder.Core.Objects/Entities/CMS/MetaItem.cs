using cCoder.Core.Objects.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.CMS;

[Table("MetaItems", Schema = "CMS")]
[ApiIgnore]
public class MetaItem
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Culture")]
    public string CultureId { get; set; }

    public string Context { get; set; }

    public string Type { get; set; }

    public string Operation { get; set; }

    public string Content { get; set; }

    public Culture Culture { get; set; }
}