// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Mail.Models;
using cCoder.Data.Models.Mail;

namespace cCoder.Core.Services.Foundations.Mail;

internal interface IMailManagerService
{
    ValueTask<QueuedEmail> AddQueuedEmailAsync(
        QueuedEmail newQueuedEmail,
        bool checkPrivileges = false);
}