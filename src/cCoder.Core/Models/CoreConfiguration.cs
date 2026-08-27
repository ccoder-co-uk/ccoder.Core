// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity.Models;
using cCoder.AI.Models.Configurations;
using cCoder.ContentManagement.Models;
using cCoder.Data.Models;
using cCoder.DocumentManagement.Models;
using cCoder.Eventing.Models;
using cCoder.Logging.Models;
using cCoder.Mail.Models;
using cCoder.Packaging.Models;
using cCoder.Security.Models;
using cCoder.Workflow.Models;
using System.Text.Json.Serialization;

namespace cCoder.Core.Models;

public class CoreConfiguration
{
    public AIConfiguration AI { get; set; }
    public AppSecurityConfiguration AppSecurity { get; set; }
    public ContentManagementConfiguration ContentManagement { get; set; }
    public CoreDataConfiguration CoreData { get; set; }
    public DocumentManagementConfiguration DocumentManagement { get; set; }
    public LoggingConfiguration Logging { get; set; }
    public MailConfiguration Mail { get; set; }
    public PackagingConfiguration Packaging { get; set; }
    public SecurityConfiguration Security { get; set; }
    public SecurityDataConfiguration SecurityData { get; set; }
    public WorkflowConfiguration Workflow { get; set; }
    public EventingConfiguration Eventing { get; set; }
    public ApiConfiguration Api { get; set; }

    [JsonIgnore]
    public Microsoft.Extensions.Configuration.IConfiguration ApplicationConfiguration { get; set; }
}