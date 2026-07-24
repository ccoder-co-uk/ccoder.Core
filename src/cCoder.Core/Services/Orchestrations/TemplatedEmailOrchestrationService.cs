// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity.Brokers;
using cCoder.ContentManagement.Exposures;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Foundations.Mail;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Security;
using cCoder.Mail.Models;
using cCoder.Mail.Services.Processings;
using CoreApp = cCoder.Data.Models.CMS.App;
using ContentTemplate = cCoder.Data.Models.CMS.Template;
using TemplatedEmailDetails = cCoder.Mail.Models.TemplatedEmailDetails;

namespace cCoder.Core.Services.Orchestrations;

internal sealed partial class TemplatedEmailOrchestrationService(
    IContentManagementAppService contentManagementAppService,
    ITemplateRenderer templateRenderer,
    IMailSenderProcessingService mailSenderProcessingService,
    IMailManagerService mailManagerService,
    IAuthorizationBroker authorizationBroker
) : ITemplatedEmailOrchestrationService
{
    public async ValueTask<QueuedEmail> QueueAsync(
        CoreApp app,
        string templateName,
        string culture,
        object model,
        string toEmail,
        string subject,
        string sentByUserId,
        string mailSenderName = "Default"
    )
    {
        ContentTemplate template = app.Templates.FirstOrDefault(predicate: candidate => candidate.Name == templateName)
            ?? throw new InvalidOperationException($"Template '{templateName}' was not found.");

        MailSender mailSender = mailSenderProcessingService
            .GetAllMailSender(ignoreFilters: true)
            .Where(predicate: sender => sender.AppId == app.Id)
            .FirstOrDefault(predicate: sender => sender.Name == mailSenderName)
            ?? mailSenderProcessingService
                .GetAllMailSender(ignoreFilters: true)
                .FirstOrDefault(predicate: sender => sender.AppId == app.Id)
            ?? throw new InvalidOperationException("Mail Sender configuration could not be found.");

        string content = templateRenderer.Render(
appId: app.Id, name: templateName, culture: culture, model: model);

        QueuedEmail email = new()
        {
            AppId = app.Id,
            MailServerName = mailSender.Name,
            MailSenderId = mailSender.Id,
            To = toEmail,
            Subject = subject,
            Content = content
                .Replace(oldValue: "[email[subject]]", newValue: subject)
                .Replace(oldValue: "[email[from]]", newValue: mailSender.FromEmail ?? mailSender.User)
                .Replace(oldValue: "[email[to]]", newValue: toEmail),
            IsBodyHtml = true,
            SentByUserId = sentByUserId,
        };

        return await mailManagerService.AddQueuedEmailAsync(
            newQueuedEmail: email,
            checkPrivileges: false);
    }

    public ValueTask<QueuedEmail> QueueAsync(TemplatedEmailDetails details)
    {
        CoreApp app = contentManagementAppService.GetAppByDomain(
            domain: details.SourceDomain,
            ignoreFilters: true);

        if (app is null)
        {
            throw new InvalidOperationException($"No app found for domain '{details.SourceDomain}'");
        }

        var currentUser = authorizationBroker.GetCurrentUser();
        string culture = ResolveCulture(details: details, currentUserCulture: currentUser?.DefaultCultureId, appCulture: app.DefaultCultureId);

        var renderModel = new
        {
            Data = details.Model,
            CoreUser = currentUser is null
                ? null
                : new
                {
                    currentUser.Id,
                    DefaultCultureId = culture,
                    currentUser.DisplayName,
                    currentUser.Email,
                    currentUser.IsActive,
                },
        };

        return QueueAsync(
app: app, templateName: details.TemplateName, culture: culture, model: renderModel, toEmail: details.ToEmail, subject: $"{app.Name}: {details.Subject}", sentByUserId: currentUser?.Id);
    }

    private static string ResolveCulture(
        TemplatedEmailDetails details,
        string currentUserCulture,
        string appCulture
    ) =>
        !string.IsNullOrWhiteSpace(value: details.Culture)
            ? details.Culture
            : !string.IsNullOrWhiteSpace(value: currentUserCulture)
                ? currentUserCulture
                : appCulture;
}