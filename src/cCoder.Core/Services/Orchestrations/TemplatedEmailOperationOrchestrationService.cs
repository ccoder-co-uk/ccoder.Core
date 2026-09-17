// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.Core.Services.Foundations.TemplatedEmails;
using CoreApp = cCoder.Data.Models.CMS.App;
using QueuedEmail = cCoder.Data.Models.Mail.QueuedEmail;
using TemplatedEmailDetails = cCoder.Mail.Models.TemplatedEmailDetails;

namespace cCoder.Core.Services.Orchestrations;

internal sealed partial class TemplatedEmailOrchestrationService(
    ITemplatedEmailContentService templatedEmailContentService,
    ITemplatedEmailIdentityService templatedEmailIdentityService,
    ITemplatedEmailQueueService templatedEmailQueueService
) : ITemplatedEmailOperationOrchestrationService,
    ITemplatedEmailOrchestrationService
{
    public ValueTask<TemplatedEmailOperation> QueueTemplatedEmailOperationAsync(
        TemplatedEmailOperation templatedEmailOperation) =>
        TryCatch(operation: async () =>
        {
            ValidateTemplatedEmailOperationOnQueue(
                templatedEmailOperation: templatedEmailOperation);

            templatedEmailContentService.ResolveTemplatedEmailOperationContent(
                templatedEmailOperation: templatedEmailOperation);

            templatedEmailIdentityService.ResolveTemplatedEmailOperationIdentity(
                templatedEmailOperation: templatedEmailOperation);

            templatedEmailContentService.RenderTemplatedEmailOperationContent(
                templatedEmailOperation: templatedEmailOperation);

            return await templatedEmailQueueService
                .QueueTemplatedEmailOperationAsync(
                    templatedEmailOperation: templatedEmailOperation);
        });

    ValueTask<QueuedEmail>
        ITemplatedEmailOrchestrationService.QueueAppTemplatedEmailAsync(
            CoreApp app,
            string templateName,
            string culture,
            object model,
            string toEmail,
            string subject,
            string sentByUserId,
            string mailSenderName)
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

        return QueueTemplatedEmailAsync(
            templatedEmailOperation: templatedEmailOperation);
    }

    ValueTask<QueuedEmail>
        ITemplatedEmailOrchestrationService.QueueTemplatedEmailDetailsAsync(
            TemplatedEmailDetails details)
    {
        TemplatedEmailOperation templatedEmailOperation = new()
        {
            Details = details,
        };

        return QueueTemplatedEmailAsync(
            templatedEmailOperation: templatedEmailOperation);
    }

    private async ValueTask<QueuedEmail> QueueTemplatedEmailAsync(
        TemplatedEmailOperation templatedEmailOperation)
    {
        TemplatedEmailOperation completedOperation =
            await QueueTemplatedEmailOperationAsync(
                templatedEmailOperation: templatedEmailOperation);

        return completedOperation.Email;
    }
}