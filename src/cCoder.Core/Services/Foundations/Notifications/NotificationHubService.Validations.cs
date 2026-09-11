// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Core.Models.Notifications;

namespace cCoder.Core.Services.Foundations.Notifications;

internal sealed partial class NotificationHubService
{
    private static void ValidateNotificationHubOperationOnExecute(
        NotificationHubOperation notificationHubOperation) =>
        ValidationRulesEngine.Validate(inputs: [notificationHubOperation]);
}