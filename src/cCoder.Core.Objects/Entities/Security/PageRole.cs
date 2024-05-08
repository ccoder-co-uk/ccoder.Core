using cCoder.Core.Objects.Entities.CMS;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.Security;

[Table("PageRoles", Schema = "Security")]
public class PageRole
{
    [ForeignKey("Page")]
    public int PageId { get; set; }

    [ForeignKey("Role")]
    public Guid RoleId { get; set; }

    public virtual Page Page { get; set; }

    public virtual Role Role { get; set; }
}