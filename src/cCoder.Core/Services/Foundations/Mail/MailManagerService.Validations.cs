// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Data.Models.Mail;

namespace cCoder.Core.Services.Foundations.Mail;

internal sealed partial class MailManagerService
{
    private static void ValidateQueuedEmailOnAdd(
        QueuedEmail newQueuedEmail,
        bool checkPrivileges) =>
        ValidationRulesEngine.Validate(
            inputs: [newQueuedEmail, checkPrivileges]);
}