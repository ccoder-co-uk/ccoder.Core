// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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
accountEvent: accountEvent, templateName: "ConfirmRegistration", subject: "Confirm Registration");

    public ValueTask QueueInvitationCreatedEmailAsync(SecurityAccountEvent accountEvent) =>
        QueueAccountEmailAsync(
accountEvent: accountEvent, templateName: "UserInvite", subject: "Confirm Invitation");

    public ValueTask QueuePasswordResetRequestedEmailAsync(SecurityAccountEvent accountEvent) =>
        QueueAccountEmailAsync(
accountEvent: accountEvent, templateName: "ForgotPassword", subject: "Password Reset");

    private async ValueTask QueueAccountEmailAsync(
        SecurityAccountEvent accountEvent,
        string templateName,
        string subject)
    {
        if (string.IsNullOrWhiteSpace(value: accountEvent?.RequestDomain))
        {
            return;
        }

        ValidateAccountEvent(accountEvent: accountEvent);

        App app = ResolveApp(requestDomain: accountEvent.RequestDomain);

        if (app is null)
        {
            return;
        }

        Template template = app.Templates?.FirstOrDefault(predicate: candidate =>
            candidate.Name == templateName);

        if (template is null)
        {
            return;
        }

        string culture = string.IsNullOrWhiteSpace(value: accountEvent.Culture)
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
            EncodedToken = HttpUtility.UrlEncode(str: accountEvent.Token),
            SSOUser = user,
            CoreUser = coreUser,
            accountEvent.Tenant,
            accountEvent.RequestDomain,
            accountEvent.Kind,
        };

        await templatedEmailOrchestrationService.QueueAsync(
app: app, templateName: template.Name, culture: culture, model: renderModel, toEmail: user.Email, subject: $"{app.Name}: {subject}", sentByUserId: user.Id);
    }

    private App ResolveApp(string requestDomain)
    {
        string normalizedDomain = NormalizeDomain(domain: requestDomain);

        App app = contentManagementAppService.GetAll(ignoreFilters: true)
            .Include(navigationPropertyPath: candidate => candidate.Templates)
            .FirstOrDefault(predicate: candidate => candidate.Domain == normalizedDomain)
            ?? contentManagementAppService.GetAll(ignoreFilters: true)
                .Include(navigationPropertyPath: candidate => candidate.Templates)
                .AsEnumerable()
                .FirstOrDefault(predicate: candidate =>
                    string.Equals(
a: NormalizeDomain(domain: candidate.Domain), b: normalizedDomain, comparisonType: StringComparison.OrdinalIgnoreCase));

        return app;
    }

    private static void ValidateAccountEvent(SecurityAccountEvent accountEvent)
    {
        if (accountEvent?.User is null)
        {
            throw new ValidationException("Security account event user is required.");
        }

        if (string.IsNullOrWhiteSpace(value: accountEvent.User.Email))
        {
            throw new ValidationException("Security account event user email is required.");
        }
    }

    private static string NormalizeDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(value: domain))
        {
            return string.Empty;
        }

        string candidate = domain.Trim();

        if (Uri.TryCreate(uriString: candidate, uriKind: UriKind.Absolute, result: out Uri uri))
        {
            candidate = uri.Host;
        }

        int portIndex = candidate.IndexOf(value: ':');

        if (portIndex >= 0)
        {
            candidate = candidate[..portIndex];
        }

        return candidate.Trim()
            .TrimEnd(trimChar: '/')
            .ToLowerInvariant();
    }
}