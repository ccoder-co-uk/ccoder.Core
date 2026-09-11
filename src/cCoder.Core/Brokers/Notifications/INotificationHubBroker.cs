// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Notifications;

namespace cCoder.Core.Brokers.Notifications;

internal interface INotificationHubBroker
{
    Task ExecuteNotificationHubOperationAsync(
        NotificationHubOperation notificationHubOperation);
}
