// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Mail.Models;
using cCoder.Data.Models.Mail;

namespace cCoder.Core.Brokers.Mail;

internal interface IMailManagerBroker
{
    ValueTask<QueuedEmail> AddQueuedEmailAsync(
        QueuedEmail newQueuedEmail,
        bool checkPrivileges = false);
}