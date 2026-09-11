// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Notifications;
using cCoder.Core.Models.Notifications;

namespace cCoder.Core.Services.Foundations.Notifications;

internal sealed partial class NotificationHubService(
    INotificationHubBroker notificationHubBroker)
    : INotificationHubService
{
    public Task ExecuteNotificationHubOperationAsync(
        NotificationHubOperation notificationHubOperation) =>
        TryCatch(operation: async () =>
        {
            ValidateNotificationHubOperationOnExecute(
                notificationHubOperation: notificationHubOperation);

            await notificationHubBroker.ExecuteNotificationHubOperationAsync(
                notificationHubOperation: notificationHubOperation);
        });
}