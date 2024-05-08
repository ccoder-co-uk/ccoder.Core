using cCoder.Core.Objects.Attributes;
using cCoder.Core.Objects.Entities.CMS;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.Security;

[Table("Roles", Schema = "Security")]
public class Role
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey("App")]
    public int AppId { get; set; }

    [Required]
    public string Name { get; set; }

    public string Description { get; set; }

    public string Privs { get; set; }

    public App App { get; set; }

    public virtual ICollection<UserRole> Users { get; set; }

    public virtual ICollection<PageRole> Pages { get; set; }

    public virtual ICollection<FolderRole> Folders { get; set; }

    public virtual ICollection<string> Privileges { get => Privs.Split(","); set => Privs = string.Join(',', value); }

    [DontPrivilege]
    public bool Allows(User user, string to) => user.Roles.Any(r => r.RoleId == Id) && Privileges.Any(p => p == to.ToLower());
}