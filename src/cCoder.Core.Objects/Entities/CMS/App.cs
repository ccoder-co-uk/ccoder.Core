using cCoder.Core.Objects.Attributes;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Objects.Entities.Mail;
using cCoder.Core.Objects.Entities.Planning;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Entities.Workflow;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;

namespace cCoder.Core.Objects.Entities.CMS;

[Table("Apps", Schema = "CMS")]
public class App
{
    [Key]
    public int Id { get; set; }

    [Required(AllowEmptyStrings = true)]
    public string DefaultCultureId { get; set; }

    public string TenantId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Domain { get; set; }

    [Required]
    public string DefaultTheme { get; set; }

    public string ConfigJson { get; set; }

    [ApiIgnore]
    [JsonIgnore]
    public dynamic Config { get => Data.ParseJson<ExpandoObject>(ConfigJson ?? "{}"); set => ConfigJson = value.ToJson(); }

    public virtual ICollection<AppCulture> Cultures { get; set; }
    public virtual ICollection<Page> Pages { get; set; }
    public virtual ICollection<Component> Components { get; set; }
    public virtual ICollection<Script> Scripts { get; set; }
    public virtual ICollection<Role> Roles { get; set; }
    public virtual ICollection<Template> Templates { get; set; }
    public virtual ICollection<Resource> Resources { get; set; }
    public virtual ICollection<ScheduledTask> Tasks { get; set; }
    public virtual ICollection<Calendar> Calendars { get; set; }
    public virtual ICollection<Folder> Folders { get; set; }
    public virtual ICollection<Layout> Layouts { get; set; }
    public virtual ICollection<FlowDefinition> Flows { get; set; }
    public virtual ICollection<MailServer> MailServers { get; set; }
    public virtual ICollection<QueuedEmail> MailQueue { get; set; }
    public virtual ICollection<SentEmail> SentMail { get; set; }

    [DontPrivilege]
    public bool IsAppAdmin(User user) => user.Roles?.Any(r => r.Role.AppId == Id && r.Role.Allows(user, "App_Admin")) ?? false;

    [DontPrivilege]
    public bool IsAppUser(User user) => user.Id != "Guest" && Roles.Any(r => r.Users.Any(u => u.User.Id == user.Id));
}