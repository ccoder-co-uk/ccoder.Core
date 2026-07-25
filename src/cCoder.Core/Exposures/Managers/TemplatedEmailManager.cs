// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Orchestrations;
using cCoder.Core.Models;
using CoreApp = cCoder.Data.Models.CMS.App;
using QueuedEmail = cCoder.Data.Models.Mail.QueuedEmail;
using TemplatedEmailDetails = cCoder.Mail.Models.TemplatedEmailDetails;

namespace cCoder.Core.Exposures.Managers;

internal sealed class TemplatedEmailManager(
    ITemplatedEmailOperationOrchestrationService
        templatedEmailOperationOrchestrationService
) : ITemplatedEmailManager, ITemplatedEmailOrchestrationService
{
    public async ValueTask<QueuedEmail> QueueAppTemplatedEmailAsync(
        CoreApp app,
        string templateName,
        string culture,
        object model,
        string toEmail,
        string subject,
        string sentByUserId,
        string mailSenderName = "Default")
    {
        TemplatedEmailOperation templatedEmailOperation = new()
        {
            App = app,
            TemplateName = templateName,
            Culture = culture,
            Model = model,
            ToEmail = toEmail,
            Subject = subject,
            SentByUserId = sentByUserId,
            MailSenderName = mailSenderName,
        };

        TemplatedEmailOperation completedOperation =
            await templatedEmailOperationOrchestrationService
                .QueueTemplatedEmailOperationAsync(
                    templatedEmailOperation: templatedEmailOperation);

        return completedOperation.Email;
    }

    public async ValueTask<QueuedEmail> QueueTemplatedEmailDetailsAsync(
        TemplatedEmailDetails details)
    {
        TemplatedEmailOperation templatedEmailOperation = new()
        {
            Details = details,
        };

        TemplatedEmailOperation completedOperation =
            await templatedEmailOperationOrchestrationService
                .QueueTemplatedEmailOperationAsync(
                    templatedEmailOperation: templatedEmailOperation);

        return completedOperation.Email;
    }
}