using cCoder.Core.Objects.Attributes;
using cCoder.Core.Objects.Entities.CMS;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.Security;

[Table("Users", Schema = "Security")]
public class User
{
    [Key]
    public string Id { get; set; }

    [ForeignKey("DefaultCulture")]
    [Required(AllowEmptyStrings = true)]
    public string DefaultCultureId { get; set; }

    [Required]
    public string DisplayName { get; set; }

    [Required]
    public string Email { get; set; }

    public bool IsActive { get; set; }

    public virtual Culture DefaultCulture { get; set; }

    public virtual ICollection<UserRole> Roles { get; set; }

    [DontPrivilege]
    public bool IsAdminOfApp(int appId) => Roles?.Any(r => r.Role.AppId == appId && r.Role.Allows(this, "app_admin")) ?? false;

    [DontPrivilege]
    public bool IsUserOfApp(int appId) => Roles?.Any(r => r.Role.App.Id == appId) ?? false;

    [DontPrivilege]
    public bool Can(int? appId, string operation)
    {
        operation = operation.ToLower(); // ensures a lower case check on the priv key as all privs are lower case in the db
        bool userCan = (appId != null && IsAdminOfApp((int)appId)) || (Roles?.Any(r => (appId == null || r.Role.AppId == appId) && r.Role.Privileges.Contains(operation)) ?? false);
        return userCan;
    }
}