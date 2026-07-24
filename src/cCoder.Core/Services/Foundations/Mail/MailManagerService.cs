// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Mail;
using cCoder.Data.Models.Mail;

namespace cCoder.Core.Services.Foundations.Mail;

internal sealed partial class MailManagerService(
    IMailManagerBroker mailManagerBroker)
    : IMailManagerService
{
    public ValueTask<QueuedEmail> AddQueuedEmailAsync(
        QueuedEmail newQueuedEmail,
        bool checkPrivileges = false) =>
        TryCatch(operation: () =>
        {
            ValidateQueuedEmailOnAdd(
                newQueuedEmail: newQueuedEmail,
                checkPrivileges: checkPrivileges);

            return mailManagerBroker.AddQueuedEmailAsync(
                newQueuedEmail: newQueuedEmail,
                checkPrivileges: checkPrivileges);
        });
}