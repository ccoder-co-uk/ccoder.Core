using cCoder.Core.Objects.Entities.Security;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.CMS;

[Table("Cultures", Schema = "CMS")]
public class Culture
{
    [Key]
    [Required(AllowEmptyStrings = true)]
    public string Id { get; set; }

    [Required]
    public string Name { get; set; }

    public virtual ICollection<AppCulture> Apps { get; set; }
    public virtual ICollection<User> Users { get; set; }
    public virtual ICollection<PageInfo> PageInfos { get; set; }
    public virtual ICollection<Content> PageContents { get; set; }
    public virtual ICollection<MetaItem> MetaItems { get; set; }

    public static bool operator ==(Culture a, Culture b) => a.Id == b?.Id;

    public static bool operator !=(Culture a, Culture b) => !(a == b);

    public override bool Equals(object obj) => obj is Culture culture && culture == this;

    public override int GetHashCode() => Id.GetHashCode();
}