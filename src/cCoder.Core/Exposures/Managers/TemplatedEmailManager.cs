// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Orchestrations;
using CoreApp = cCoder.Data.Models.CMS.App;
using QueuedEmail = cCoder.Data.Models.Mail.QueuedEmail;
using TemplatedEmailDetails = cCoder.Mail.Models.TemplatedEmailDetails;

namespace cCoder.Core.Exposures.Managers;

internal sealed class TemplatedEmailManager(
    TemplatedEmailOrchestrationService templatedEmailOrchestrationService
) : ITemplatedEmailManager, ITemplatedEmailOrchestrationService
{
    public ValueTask<QueuedEmail> QueueAppTemplatedEmailAsync(
        CoreApp app,
        string templateName,
        string culture,
        object model,
        string toEmail,
        string subject,
        string sentByUserId,
        string mailSenderName = "Default") =>
        templatedEmailOrchestrationService.QueueAppTemplatedEmailAsync(
            app: app,
            templateName: templateName,
            culture: culture,
            model: model,
            toEmail: toEmail,
            subject: subject,
            sentByUserId: sentByUserId,
            mailSenderName: mailSenderName);

    public ValueTask<QueuedEmail> QueueTemplatedEmailDetailsAsync(
        TemplatedEmailDetails details) =>
        templatedEmailOrchestrationService.QueueTemplatedEmailDetailsAsync(
            details: details);
}