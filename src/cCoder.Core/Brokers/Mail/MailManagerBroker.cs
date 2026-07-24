// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Mail.Exposures;
using cCoder.Mail.Models;
using cCoder.Data.Models.Mail;

namespace cCoder.Core.Brokers.Mail;

internal sealed class MailManagerBroker(
    IMailManagerExposure mailManagerExposure)
    : IMailManagerBroker
{
    public ValueTask<QueuedEmail> AddQueuedEmailAsync(
        QueuedEmail newQueuedEmail,
        bool checkPrivileges = false) =>
        mailManagerExposure.AddAsync(
            newQueuedEmail: newQueuedEmail,
            checkPrivileges: checkPrivileges);
}