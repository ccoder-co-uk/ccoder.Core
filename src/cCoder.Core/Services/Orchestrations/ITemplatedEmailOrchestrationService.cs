using CoreApp = cCoder.Data.Models.CMS.App;
using QueuedEmail = cCoder.Data.Models.Mail.QueuedEmail;
using TemplatedEmailDetails = cCoder.Mail.Models.TemplatedEmailDetails;

namespace cCoder.Core.Services.Orchestrations;

public interface ITemplatedEmailOrchestrationService
{
    ValueTask<QueuedEmail> QueueAsync(
        CoreApp app,
        string templateName,
        string culture,
        object model,
        string toEmail,
        string subject,
        string sentByUserId,
        string mailServerName = "Default"
    );

    ValueTask<QueuedEmail> QueueAsync(TemplatedEmailDetails details);
}

