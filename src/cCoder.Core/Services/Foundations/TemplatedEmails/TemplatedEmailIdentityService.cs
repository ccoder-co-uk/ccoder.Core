// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity.Brokers;
using cCoder.Core.Models;
using cCoder.Data.Models.Mail;
using cCoder.Mail.Services.Processings;

namespace cCoder.Core.Services.Foundations.TemplatedEmails;

internal sealed partial class TemplatedEmailIdentityService(
    IAuthorizationBroker authorizationBroker,
    IMailSenderProcessingService mailSenderProcessingService
) : ITemplatedEmailIdentityService
{
    public TemplatedEmailOperation ResolveTemplatedEmailOperationIdentity(
        TemplatedEmailOperation templatedEmailOperation) =>
        TryCatch(operation: () =>
        {
            ValidateTemplatedEmailOperationOnResolve(
                templatedEmailOperation: templatedEmailOperation);

            if (templatedEmailOperation.Details is not null)
            {
                templatedEmailOperation.CurrentUser =
                    authorizationBroker.GetCurrentUser();

                templatedEmailOperation.Culture = ResolveCulture(
                    detailsCulture: templatedEmailOperation.Details.Culture,
                    currentUserCulture:
                        templatedEmailOperation.CurrentUser?.DefaultCultureId,
                    appCulture:
                        templatedEmailOperation.App.DefaultCultureId);

                templatedEmailOperation.Subject =
                    $"{templatedEmailOperation.App.Name}: {templatedEmailOperation.Details.Subject}";

                templatedEmailOperation.SentByUserId =
                    templatedEmailOperation.CurrentUser?.Id;

                templatedEmailOperation.MailSenderName = "Default";
            }

            templatedEmailOperation.MailSender =
                ResolveMailSender(
                    appId: templatedEmailOperation.App.Id,
                    mailSenderName: templatedEmailOperation.MailSenderName);

            return templatedEmailOperation;
        });

    private MailSender ResolveMailSender(
        int appId,
        string mailSenderName) =>
        mailSenderProcessingService
            .GetAllMailSender(ignoreFilters: true)
            .Where(predicate: sender => sender.AppId == appId)
            .FirstOrDefault(predicate: sender =>
                sender.Name == mailSenderName)
        ?? mailSenderProcessingService
            .GetAllMailSender(ignoreFilters: true)
            .FirstOrDefault(predicate: sender =>
                sender.AppId == appId)
        ?? throw new InvalidOperationException(
            "Mail Sender configuration could not be found.");

    private static string ResolveCulture(
        string detailsCulture,
        string currentUserCulture,
        string appCulture) =>
        !string.IsNullOrWhiteSpace(value: detailsCulture)
            ? detailsCulture
            : !string.IsNullOrWhiteSpace(value: currentUserCulture)
                ? currentUserCulture
                : appCulture;
}