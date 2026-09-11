// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.Notifications;
using cCoder.Core.Models.Notifications;

namespace cCoder.Core.Brokers.Notifications;

internal sealed class NotificationHubBroker : INotificationHubBroker
{
    public Task ExecuteNotificationHubOperationAsync(
        NotificationHubOperation notificationHubOperation) =>
        NotificationHubDependency.ExecuteNotificationHubOperationAsync(
            notificationHubOperation: notificationHubOperation);
}