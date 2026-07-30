// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Web;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Exposures.Managers;
using cCoder.Data.Models.CMS;
using cCoder.Security.Models.Entities;
using cCoder.Security.Models.Events;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Services.Aggregations;

internal sealed partial class SecurityAccountEmailAggregationService(
    IContentManagementAppService contentManagementAppService,
    ITemplatedEmailManager templatedEmailManager)
    : ISecurityAccountEmailAggregationService
{
    public ValueTask QueueRegistrationCreatedSecurityAccountEventEmailAsync(
        SecurityAccountEvent accountEvent) =>
        TryCatch(operation: async () =>
        {
            ValidateSecurityAccountEventOnQueue(accountEvent: accountEvent);

            await QueueAccountEmailAsync(
                accountEvent: accountEvent,
                templateName: "ConfirmRegistration",
                subject: "Confirm Registration");
        });

    public ValueTask QueueInvitationCreatedSecurityAccountEventEmailAsync(
        SecurityAccountEvent accountEvent) =>
        TryCatch(operation: async () =>
        {
            ValidateSecurityAccountEventOnQueue(accountEvent: accountEvent);

            await QueueAccountEmailAsync(
                accountEvent: accountEvent,
                templateName: "UserInvite",
                subject: "Confirm Invitation");
        });

    public ValueTask QueuePasswordResetRequestedSecurityAccountEventEmailAsync(
        SecurityAccountEvent accountEvent) =>
        TryCatch(operation: async () =>
        {
            ValidateSecurityAccountEventOnQueue(accountEvent: accountEvent);

            await QueueAccountEmailAsync(
                accountEvent: accountEvent,
                templateName: "ForgotPassword",
                subject: "Password Reset");
        });

    private async ValueTask QueueAccountEmailAsync(
        SecurityAccountEvent accountEvent,
        string templateName,
        string subject)
    {
        if (string.IsNullOrWhiteSpace(value: accountEvent?.RequestDomain))
        {
            return;
        }

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

        await templatedEmailManager.QueueAppTemplatedEmailAsync(
            app: app,
            templateName: template.Name,
            culture: culture,
            model: renderModel,
            toEmail: user.Email,
            subject: $"{app.Name}: {subject}",
            sentByUserId: user.Id);
    }

    private App ResolveApp(string requestDomain)
    {
        string normalizedDomain = NormalizeDomain(domain: requestDomain);

        App app = contentManagementAppService.GetAllApps(ignoreFilters: true)
            .Include(navigationPropertyPath: candidate => candidate.Templates)
            .FirstOrDefault(predicate: candidate => candidate.Domain == normalizedDomain)
            ?? contentManagementAppService.GetAllApps(ignoreFilters: true)
                .Include(navigationPropertyPath: candidate => candidate.Templates)
                .AsEnumerable()
                .FirstOrDefault(predicate: candidate =>
                    string.Equals(
a: NormalizeDomain(domain: candidate.Domain), b: normalizedDomain, comparisonType: StringComparison.OrdinalIgnoreCase));

        return app;
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