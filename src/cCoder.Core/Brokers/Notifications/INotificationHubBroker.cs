// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;
using cCoder.Core.Models.Notifications;

namespace cCoder.Core.Brokers.Notifications;

internal interface INotificationHubBroker
{
    Task ExecuteNotificationHubOperationAsync(
        NotificationHubOperation notificationHubOperation);
}
