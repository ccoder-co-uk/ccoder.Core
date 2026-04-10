using System.Dynamic;
using Newtonsoft.Json;
using ContentAppCulture = cCoder.Data.Models.CMS.AppCulture;
using ContentComponent = cCoder.Data.Models.CMS.Component;
using ContentLayout = cCoder.Data.Models.CMS.Layout;
using ContentPage = cCoder.Data.Models.CMS.Page;
using ContentResource = cCoder.Data.Models.CMS.Resource;
using ContentScript = cCoder.Data.Models.CMS.Script;
using ContentTemplate = cCoder.Data.Models.CMS.Template;
using DocumentFolder = cCoder.Data.Models.DMS.Folder;
using MailServer = cCoder.Data.Models.Mail.MailServer;
using QueuedEmail = cCoder.Data.Models.Mail.QueuedEmail;
using SchedulingCalendar = cCoder.Data.Models.Planning.Calendar;
using SchedulingTask = cCoder.Data.Models.Planning.ScheduledTask;
using SecurityRole = cCoder.Data.Models.Security.Role;
using SentEmail = cCoder.Data.Models.Mail.SentEmail;
using WorkflowFlowDefinition = cCoder.Data.Models.Workflow.FlowDefinition;

namespace cCoder.Core.Models;

public class App
{
    public int Id { get; set; }
    public string DefaultCultureId { get; set; }
    public string TenantId { get; set; }
    public string Name { get; set; }
    public string Domain { get; set; }
    public string DefaultTheme { get; set; }
    public string ConfigJson { get; set; }

    [JsonIgnore]
    public dynamic Config
    {
        get => JsonConvert.DeserializeObject<ExpandoObject>(ConfigJson ?? "{}");
        set => ConfigJson = JsonConvert.SerializeObject(value);
    }

    public virtual ICollection<ContentAppCulture> Cultures { get; set; }
    public virtual ICollection<ContentPage> Pages { get; set; }
    public virtual ICollection<ContentComponent> Components { get; set; }
    public virtual ICollection<ContentScript> Scripts { get; set; }
    public virtual ICollection<SecurityRole> Roles { get; set; }
    public virtual ICollection<ContentTemplate> Templates { get; set; }
    public virtual ICollection<ContentResource> Resources { get; set; }
    public virtual ICollection<SchedulingTask> Tasks { get; set; }
    public virtual ICollection<SchedulingCalendar> Calendars { get; set; }
    public virtual ICollection<DocumentFolder> Folders { get; set; }
    public virtual ICollection<ContentLayout> Layouts { get; set; }
    public virtual ICollection<WorkflowFlowDefinition> Flows { get; set; }
    public virtual ICollection<MailServer> MailServers { get; set; }
    public virtual ICollection<QueuedEmail> MailQueue { get; set; }
    public virtual ICollection<SentEmail> SentMail { get; set; }
}


