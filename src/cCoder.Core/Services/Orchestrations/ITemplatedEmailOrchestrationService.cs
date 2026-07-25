// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using CoreApp = cCoder.Data.Models.CMS.App;
using QueuedEmail = cCoder.Data.Models.Mail.QueuedEmail;
using TemplatedEmailDetails = cCoder.Mail.Models.TemplatedEmailDetails;

namespace cCoder.Core.Services.Orchestrations;

public interface ITemplatedEmailOrchestrationService
{
    ValueTask<QueuedEmail> QueueAppTemplatedEmailAsync(
        CoreApp app,
        string templateName,
        string culture,
        object model,
        string toEmail,
        string subject,
        string sentByUserId,
        string mailSenderName = "Default"
    );

    ValueTask<QueuedEmail> QueueTemplatedEmailDetailsAsync(
        TemplatedEmailDetails details);

    ValueTask<QueuedEmail> QueueAsync(
        CoreApp app,
        string templateName,
        string culture,
        object model,
        string toEmail,
        string subject,
        string sentByUserId,
        string mailSenderName = "Default") =>
        QueueAppTemplatedEmailAsync(
            app: app,
            templateName: templateName,
            culture: culture,
            model: model,
            toEmail: toEmail,
            subject: subject,
            sentByUserId: sentByUserId,
            mailSenderName: mailSenderName);

    ValueTask<QueuedEmail> QueueAsync(TemplatedEmailDetails details) =>
        QueueTemplatedEmailDetailsAsync(details: details);
}