// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity.Models;
using cCoder.AI.Models.Configurations;
using cCoder.ContentManagement.Models;
using cCoder.DocumentManagement.Models;
using cCoder.Eventing.Models;
using cCoder.Logging.Models;
using cCoder.Mail.Models;
using cCoder.Packaging.Models;
using cCoder.Security.Objects;
using cCoder.Workflow.Models;

namespace cCoder.Core.Models;

public sealed class CoreConfiguration
{
    public CoreConfiguration()
    {
        AI = new AIConfiguration();
        AppSecurity = new AppSecurityConfiguration();
        ContentManagement = new ContentManagementConfiguration();
        DocumentManagement = new DocumentManagementConfiguration();
        Logging = new LoggingConfiguration();
        Mail = new MailConfiguration();
        Packaging = new PackagingConfiguration();
        Security = new SecurityConfiguration();
        Workflow = new WorkflowConfiguration();
        Eventing = new EventingConfiguration();
        Api = new ApiConfiguration();
    }

    public AIConfiguration AI { get; set; }
    public AppSecurityConfiguration AppSecurity { get; set; }
    public ContentManagementConfiguration ContentManagement { get; set; }
    public DocumentManagementConfiguration DocumentManagement { get; set; }
    public LoggingConfiguration Logging { get; set; }
    public MailConfiguration Mail { get; set; }
    public PackagingConfiguration Packaging { get; set; }
    public SecurityConfiguration Security { get; set; }
    public WorkflowConfiguration Workflow { get; set; }
    public EventingConfiguration Eventing { get; set; }
    public ApiConfiguration Api { get; set; }
}