using cCoder.AppSecurity.Brokers;
using cCoder.ContentManagement.Exposures;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Foundations.Mail;
using cCoder.Mail.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Security;
using cCoder.Mail.Services.Orchestrations;
using CoreApp = cCoder.Core.Models.App;
using ContentTemplate = cCoder.Data.Models.CMS.Template;
using TemplatedEmailDetails = cCoder.Mail.Models.TemplatedEmailDetails;

namespace cCoder.Core.Services.Orchestrations;

public partial class TemplatedEmailOrchestrationService(
    IContentManagementAppService contentManagementAppService,
    ITemplateRenderer templateRenderer,
    IMailServerOrchestrationService mailServerOrchestrationService,
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
        string mailServerName = "Default"
    )
    {
        ContentTemplate template = app.Templates.FirstOrDefault(candidate => candidate.Name == templateName)
            ?? throw new InvalidOperationException($"Template '{templateName}' was not found.");

        MailServer mailServer = mailServerOrchestrationService
            .GetAll(true)
            .Where(server => server.AppId == app.Id)
            .FirstOrDefault(server => server.Name == mailServerName)
            ?? mailServerOrchestrationService
                .GetAll(true)
                .FirstOrDefault(server => server.AppId == app.Id)
            ?? throw new InvalidOperationException("Mail Server configuration could not be found.");

        string content = templateRenderer.Render(
            app.Id,
            templateName,
            culture,
            model);

        QueuedEmail email = new()
        {
            AppId = app.Id,
            MailServerName = mailServer.Name,
            To = toEmail,
            Subject = subject,
            Content = content
                .Replace("[email[subject]]", subject)
                .Replace("[email[from]]", mailServer.User)
                .Replace("[email[to]]", toEmail),
            IsBodyHtml = true,
            SentByUserId = sentByUserId,
        };

        return await mailManagerService.AddAsync(email, false);
    }

    public ValueTask<QueuedEmail> QueueAsync(TemplatedEmailDetails details)
    {
        CoreApp app = contentManagementAppService.GetByDomain(details.SourceDomain, true);

        if (app is null)
            throw new InvalidOperationException($"No app found for domain '{details.SourceDomain}'");

        var currentUser = authorizationBroker.GetCurrentUser();
        string culture = ResolveCulture(details, currentUser?.DefaultCultureId, app.DefaultCultureId);

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
            app,
            details.TemplateName,
            culture,
            renderModel,
            details.ToEmail,
            $"{app.Name}: {details.Subject}",
            currentUser?.Id);
    }

    private static string ResolveCulture(
        TemplatedEmailDetails details,
        string currentUserCulture,
        string appCulture
    ) =>
        !string.IsNullOrWhiteSpace(details.Culture)
            ? details.Culture
            : !string.IsNullOrWhiteSpace(currentUserCulture)
                ? currentUserCulture
                : appCulture;
}

