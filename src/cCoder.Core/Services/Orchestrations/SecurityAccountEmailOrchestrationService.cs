using System.ComponentModel.DataAnnotations;
using System.Web;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Data.Models.CMS;
using cCoder.Security.Objects.Entities;
using cCoder.Security.Objects.Events;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Services.Orchestrations;

public class SecurityAccountEmailOrchestrationService(
    IContentManagementAppService contentManagementAppService,
    ITemplatedEmailOrchestrationService templatedEmailOrchestrationService)
    : ISecurityAccountEmailOrchestrationService
{
    public ValueTask QueueRegistrationCreatedEmailAsync(SecurityAccountEvent accountEvent) =>
        QueueAccountEmailAsync(
            accountEvent,
            "ConfirmRegistration",
            "Confirm Registration");

    public ValueTask QueueInvitationCreatedEmailAsync(SecurityAccountEvent accountEvent) =>
        QueueAccountEmailAsync(
            accountEvent,
            "UserInvite",
            "Confirm Invitation");

    public ValueTask QueuePasswordResetRequestedEmailAsync(SecurityAccountEvent accountEvent) =>
        QueueAccountEmailAsync(
            accountEvent,
            "ForgotPassword",
            "Password Reset");

    private async ValueTask QueueAccountEmailAsync(
        SecurityAccountEvent accountEvent,
        string templateName,
        string subject)
    {
        if (string.IsNullOrWhiteSpace(accountEvent?.RequestDomain))
            return;

        ValidateAccountEvent(accountEvent);

        App app = ResolveApp(accountEvent.RequestDomain);

        if (app is null)
            return;

        Template template = app.Templates?.FirstOrDefault(candidate =>
            candidate.Name == templateName);

        if (template is null)
            return;

        string culture = string.IsNullOrWhiteSpace(accountEvent.Culture)
            ? app.DefaultCultureId
            : accountEvent.Culture;

        SSOUser user = accountEvent.User;
        var coreUser = new
        {
            user.Id,
            user.Email,
            user.DisplayName,
            DefaultCultureId = culture,
            IsActive = !user.LockoutEnabled,
        };
        var renderModel = new
        {
            accountEvent.Token,
            EncodedToken = HttpUtility.UrlEncode(accountEvent.Token),
            SSOUser = user,
            CoreUser = coreUser,
            accountEvent.Tenant,
            accountEvent.RequestDomain,
            accountEvent.Kind,
        };

        await templatedEmailOrchestrationService.QueueAsync(
            app,
            template.Name,
            culture,
            renderModel,
            user.Email,
            $"{app.Name}: {subject}",
            user.Id);
    }

    private App ResolveApp(string requestDomain)
    {
        string normalizedDomain = NormalizeDomain(requestDomain);

        App app = contentManagementAppService.GetAll(true)
            .Include(candidate => candidate.Templates)
            .FirstOrDefault(candidate => candidate.Domain == normalizedDomain)
            ?? contentManagementAppService.GetAll(true)
                .Include(candidate => candidate.Templates)
                .AsEnumerable()
                .FirstOrDefault(candidate =>
                    string.Equals(
                        NormalizeDomain(candidate.Domain),
                        normalizedDomain,
                        StringComparison.OrdinalIgnoreCase));

        return app;
    }

    private static void ValidateAccountEvent(SecurityAccountEvent accountEvent)
    {
        if (accountEvent?.User is null)
            throw new ValidationException("Security account event user is required.");

        if (string.IsNullOrWhiteSpace(accountEvent.User.Email))
            throw new ValidationException("Security account event user email is required.");

    }

    private static string NormalizeDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return string.Empty;

        string candidate = domain.Trim();

        if (Uri.TryCreate(candidate, UriKind.Absolute, out Uri uri))
            candidate = uri.Host;

        int portIndex = candidate.IndexOf(':');

        if (portIndex >= 0)
            candidate = candidate[..portIndex];

        return candidate.Trim().TrimEnd('/').ToLowerInvariant();
    }
}
