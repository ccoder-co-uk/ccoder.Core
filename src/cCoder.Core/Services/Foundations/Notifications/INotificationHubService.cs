// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;
using cCoder.Core.Models.Notifications;

namespace cCoder.Core.Services.Foundations.Notifications;

internal interface INotificationHubService
{
    Task ExecuteNotificationHubOperationAsync(
        NotificationHubOperation notificationHubOperation);
}