// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity.Models;
using cCoder.AI.Models.Configurations;
using cCoder.ContentManagement.Models;
using cCoder.ClientRelationshipManagement.Platform.Models.Configuration;
using cCoder.DocumentManagement.Models;
using cCoder.Eventing.Models;
using cCoder.Logging.Models;
using cCoder.Mail.Models;
using cCoder.Packaging.Models;
using cCoder.Security.Models;
using cCoder.Workflow.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

namespace cCoder.Core.Models;

public sealed class CoreConfiguration
{
    public CoreConfiguration()
    {
        Eventing = new EventingConfiguration();
        Api = new ApiConfiguration();
    }

    public CoreConfiguration(IConfiguration configuration)
        : this()
    {
        ArgumentNullException.ThrowIfNull(argument: configuration);

        IConfigurationSection ai =
            configuration.GetSection(key: nameof(AI));

        IConfigurationSection appSecurity =
            configuration.GetSection(key: nameof(AppSecurity));

        IConfigurationSection contentManagement =
            configuration.GetSection(key: nameof(ContentManagement));

        IConfigurationSection crm =
            configuration.GetSection(key: nameof(CRM));

        IConfigurationSection documentManagement =
            configuration.GetSection(key: nameof(DocumentManagement));

        IConfigurationSection logging =
            configuration.GetSection(key: nameof(Logging));

        IConfigurationSection mail =
            configuration.GetSection(key: nameof(Mail));

        IConfigurationSection packaging =
            configuration.GetSection(key: nameof(Packaging));

        IConfigurationSection security =
            configuration.GetSection(key: nameof(Security));

        IConfigurationSection workflow =
            configuration.GetSection(key: nameof(Workflow));

        AI = ai.Exists() ? ai.Get<AIConfiguration>() : null;

        AppSecurity = appSecurity.Exists()
            ? appSecurity.Get<AppSecurityConfiguration>()
            : null;

        ContentManagement = contentManagement.Exists()
            ? contentManagement.Get<ContentManagementConfiguration>()
            : null;

        CRM = crm.Exists()
            ? crm.Get<CRMConfiguration>()
            : null;

        DocumentManagement = documentManagement.Exists()
            ? documentManagement.Get<DocumentManagementConfiguration>()
            : null;

        Logging = logging.Exists()
            ? logging.Get<LoggingConfiguration>()
            : null;

        Mail = mail.Exists() ? mail.Get<MailConfiguration>() : null;

        Packaging = packaging.Exists()
            ? packaging.Get<PackagingConfiguration>()
            : null;

        Security = security.Exists()
            ? security.Get<SecurityConfiguration>()
            : null;

        Workflow = workflow.Exists()
            ? workflow.Get<WorkflowConfiguration>()
            : null;

        configuration.GetSection(key: nameof(Eventing))
            .Bind(instance: Eventing);

        configuration.GetSection(key: nameof(Api))
            .Bind(instance: Api);

        ApplicationConfiguration = configuration;
    }

    public AIConfiguration AI { get; set; }
    public AppSecurityConfiguration AppSecurity { get; set; }
    public ContentManagementConfiguration ContentManagement { get; set; }
    public CRMConfiguration CRM { get; set; }
    public DocumentManagementConfiguration DocumentManagement { get; set; }
    public LoggingConfiguration Logging { get; set; }
    public MailConfiguration Mail { get; set; }
    public PackagingConfiguration Packaging { get; set; }
    public SecurityConfiguration Security { get; set; }
    public WorkflowConfiguration Workflow { get; set; }
    public EventingConfiguration Eventing { get; set; }
    public ApiConfiguration Api { get; set; }

    [JsonIgnore]
    public IConfiguration ApplicationConfiguration { get; set; }
}