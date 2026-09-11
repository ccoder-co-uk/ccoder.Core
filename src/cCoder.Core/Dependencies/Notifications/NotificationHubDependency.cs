// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace cCoder.Core.Dependencies.Notifications;

internal static class NotificationHubDependency
{
    internal static Task ExecuteNotificationHubOperationAsync(
        NotificationHubOperation notificationHubOperation) =>
        notificationHubOperation.Kind switch
        {
            NotificationHubOperationKind.AddToGroup =>
                ((IGroupManager)notificationHubOperation.Groups).AddToGroupAsync(
                    connectionId: notificationHubOperation.ConnectionId,
                    groupName: notificationHubOperation.Thread),
            NotificationHubOperationKind.RemoveFromGroup =>
                ((IGroupManager)notificationHubOperation.Groups).RemoveFromGroupAsync(
                    connectionId: notificationHubOperation.ConnectionId,
                    groupName: notificationHubOperation.Thread),
            NotificationHubOperationKind.SendToCaller =>
                ((IHubCallerClients)notificationHubOperation.Clients).Caller.SendCoreAsync(
                    method: notificationHubOperation.Method,
                    args: notificationHubOperation.Arguments),
            _ => ((IHubCallerClients)notificationHubOperation.Clients)
                .Group(groupName: notificationHubOperation.Thread)
                .SendCoreAsync(
                    method: notificationHubOperation.Method,
                    args: notificationHubOperation.Arguments)
        };
}
