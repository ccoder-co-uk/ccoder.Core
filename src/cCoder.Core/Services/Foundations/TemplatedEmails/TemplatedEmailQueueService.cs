// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.Core.Brokers.Mail;
using cCoder.Data.Models.Mail;
using cCoder.Mail.Services.Processings;

namespace cCoder.Core.Services.Foundations.TemplatedEmails;

internal sealed partial class TemplatedEmailQueueService(
    IMailManagerBroker mailManagerBroker
) : ITemplatedEmailQueueService
{
    public ValueTask<TemplatedEmailOperation> QueueTemplatedEmailOperationAsync(
        TemplatedEmailOperation templatedEmailOperation) =>
        TryCatch(operation: async () =>
        {
            ValidateTemplatedEmailOperationOnQueue(
                templatedEmailOperation: templatedEmailOperation);

            MailSender mailSender = templatedEmailOperation.MailSender;

            QueuedEmail email = new()
            {
                AppId = templatedEmailOperation.App.Id,
                MailServerName = mailSender.Name,
                MailSenderId = mailSender.Id,
                To = templatedEmailOperation.ToEmail,
                Subject = templatedEmailOperation.Subject,
                Content = templatedEmailOperation.Content
                    .Replace(
                        oldValue: "[email[subject]]",
                        newValue: templatedEmailOperation.Subject)
                    .Replace(
                        oldValue: "[email[from]]",
                        newValue: mailSender.FromEmail ?? mailSender.User)
                    .Replace(
                        oldValue: "[email[to]]",
                        newValue: templatedEmailOperation.ToEmail),
                IsBodyHtml = true,
                SentByUserId = templatedEmailOperation.SentByUserId,
            };

            templatedEmailOperation.Email =
                await mailManagerBroker.AddQueuedEmailAsync(
                    newQueuedEmail: email,
                    checkPrivileges: false);

            return templatedEmailOperation;
        });
}