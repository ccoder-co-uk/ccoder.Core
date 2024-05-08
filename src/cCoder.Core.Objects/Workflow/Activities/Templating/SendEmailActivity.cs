using cCoder.Core.Objects.Dtos.Workflow;
using cCoder.Core.Objects.Extensions;

namespace cCoder.Core.Objects.Workflow.Activities.Templating;

public class SendEmailActivity : TemplatingActivity<dynamic>
{
    public string Subject { get; set; }
    public string MailServerName { get; set; }
    public string CC { get; set; }
    public string To { get; set; }

    public override async Task Execute()
    {
        using System.Net.Http.HttpClient api = GetHttpClient();
        Log(WorkflowLogLevel.Info, $"Building Email ...");
        Entities.Mail.QueuedEmail email = await BuildEmailTo(To, Subject, MailServerName, api);
        if (email != null)
        {
            Log(WorkflowLogLevel.Info, $"Email built, sending ...");
            _ = await api.AddAsync($"Core/QueuedEmail", email);
            Log(WorkflowLogLevel.Info, $"Email Sent!");
        }
    }
}