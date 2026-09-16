// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Data.Models.Mail;

namespace cCoder.Core.Services.Foundations.Mail;

internal sealed partial class MailManagerService
{
    private static void Validate(params object[] inputs) =>
        ValidationRulesEngine.Validate(inputs: inputs);

    private static void ValidateQueuedEmailOnAdd(
        QueuedEmail newQueuedEmail,
        bool checkPrivileges) =>
        Validate(
            inputs: [newQueuedEmail, checkPrivileges]);
}