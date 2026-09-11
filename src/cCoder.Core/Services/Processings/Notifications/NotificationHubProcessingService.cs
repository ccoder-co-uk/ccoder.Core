// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Loggings;
using cCoder.Core.Models.Notifications;
using cCoder.Core.Services.Foundations.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace cCoder.Core.Services.Processings.Notifications;

internal sealed partial class NotificationHubProcessingService(
    ILoggingBroker log,
    INotificationHubService notificationHubService)
    : INotificationHubProcessingService
{
    private static readonly IDictionary<string, ICollection<NotificationHistoryItem>> History =
        new Dictionary<string, ICollection<NotificationHistoryItem>>();

    private static readonly IDictionary<string, int> UserCounts =
        new Dictionary<string, int>();

    public Task OnConnectedAsync(Func<Task> onConnectedAsync) =>
        TryCatch(operation: async () =>
        {
            ValidateOnConnected(onConnectedAsync: onConnectedAsync);

            if (log.IsDebugEnabled())
            {
                log.LogDebug(
                    message: "New client connected to {HubName}",
                    args: "NotificationHub");
            }

            await onConnectedAsync();
        });

    public Task OnDisconnectedAsync(
        Exception exception,
        Func<Exception, Task> onDisconnectedAsync) =>
        TryCatch(operation: async () =>
        {
            ValidateOnDisconnected(
                exception: exception,
                onDisconnectedAsync: onDisconnectedAsync);

            if (log.IsDebugEnabled())
            {
                log.LogDebug(
                    message: "Client disconnected from {HubName}",
                    args: "NotificationHub");
            }

            await onDisconnectedAsync(arg: exception);
        });

    public Task JoinAsync(
        string thread,
        string connectionId,
        IGroupManager groups,
        IHubCallerClients clients) =>
        TryCatch(operation: async () =>
        {
            ValidateOnJoin(
                thread: thread,
                connectionId: connectionId,
                groups: groups,
                clients: clients);

            if (log.IsDebugEnabled())
            {
                log.LogDebug(
                    message: "User joining {Thread}",
                    args: thread);
            }

            await notificationHubService.ExecuteNotificationHubOperationAsync(
                notificationHubOperation: new NotificationHubOperation
                {
                    Kind = NotificationHubOperationKind.AddToGroup,
                    Groups = groups,
                    ConnectionId = connectionId,
                    Thread = thread
                });

            await notificationHubService.ExecuteNotificationHubOperationAsync(
                notificationHubOperation: new NotificationHubOperation
                {
                    Kind = NotificationHubOperationKind.SendToCaller,
                    Clients = clients,
                    Method = "ConsoleReceive",
                    Arguments = ["info", "Connected to instance " + thread, thread]
                });

            await notificationHubService.ExecuteNotificationHubOperationAsync(
                notificationHubOperation: new NotificationHubOperation
                {
                    Kind = NotificationHubOperationKind.SendToGroup,
                    Clients = clients,
                    Thread = thread,
                    Method = "ConsoleReceive",
                    Arguments = ["info", "User Joined", thread]
                });

            if (!History.TryGetValue(
                key: thread,
                value: out ICollection<NotificationHistoryItem> history))
            {
                history = [];
                History.Add(key: thread, value: history);
            }

            if (!UserCounts.TryGetValue(key: thread, value: out int userCount))
            {
                UserCounts.Add(key: thread, value: 1);
            }
            else
            {
                UserCounts[thread] = userCount + 1;
            }

            foreach (NotificationHistoryItem item in history)
            {
                await notificationHubService.ExecuteNotificationHubOperationAsync(
                    notificationHubOperation: new NotificationHubOperation
                    {
                        Kind = NotificationHubOperationKind.SendToCaller,
                        Clients = clients,
                        Method = "ConsoleReceive",
                        Arguments = [item.Level, item.Message, thread]
                    });
            }
        });

    public Task LeaveAsync(
        string thread,
        string connectionId,
        IGroupManager groups,
        IHubCallerClients clients) =>
        TryCatch(operation: async () =>
        {
            ValidateOnLeave(
                thread: thread,
                connectionId: connectionId,
                groups: groups,
                clients: clients);

            if (log.IsDebugEnabled())
            {
                log.LogDebug(
                    message: "User leaving {Thread}",
                    args: thread);
            }

            await notificationHubService.ExecuteNotificationHubOperationAsync(
                notificationHubOperation: new NotificationHubOperation
                {
                    Kind = NotificationHubOperationKind.RemoveFromGroup,
                    Groups = groups,
                    ConnectionId = connectionId,
                    Thread = thread
                });

            await notificationHubService.ExecuteNotificationHubOperationAsync(
                notificationHubOperation: new NotificationHubOperation
                {
                    Kind = NotificationHubOperationKind.SendToCaller,
                    Clients = clients,
                    Method = "info",
                    Arguments = ["Stopped listening to messages for " + thread, thread]
                });

            await notificationHubService.ExecuteNotificationHubOperationAsync(
                notificationHubOperation: new NotificationHubOperation
                {
                    Kind = NotificationHubOperationKind.SendToGroup,
                    Clients = clients,
                    Thread = thread,
                    Method = "ConsoleReceive",
                    Arguments = ["info", "User Left", thread]
                });

            UserCounts[thread]--;

            if (UserCounts[thread] == 0)
            {
                History.Remove(key: thread);
            }
        });

    public void Send(
        string level,
        string message,
        string thread,
        IHubCallerClients clients) =>
        TryCatch(operation: () =>
        {
            ValidateOnSend(
                level: level,
                message: message,
                thread: thread,
                clients: clients);

            _ = notificationHubService.ExecuteNotificationHubOperationAsync(
                notificationHubOperation: new NotificationHubOperation
                {
                    Kind = NotificationHubOperationKind.SendToGroup,
                    Clients = clients,
                    Thread = thread,
                    Method = level,
                    Arguments = [message]
                });
        });

    public Task ConsoleSendAsync(
        string level,
        string message,
        string thread,
        IHubCallerClients clients) =>
        TryCatch(operation: async () =>
        {
            ValidateOnConsoleSend(
                level: level,
                message: message,
                thread: thread,
                clients: clients);

            if (!History.TryGetValue(
                key: thread,
                value: out ICollection<NotificationHistoryItem> history))
            {
                history = [];
                History.Add(key: thread, value: history);
            }

            history.Add(
                item: new NotificationHistoryItem
                {
                    Message = message,
                    Level = level
                });

            await notificationHubService.ExecuteNotificationHubOperationAsync(
                notificationHubOperation: new NotificationHubOperation
                {
                    Kind = NotificationHubOperationKind.SendToGroup,
                    Clients = clients,
                    Thread = thread,
                    Method = "ConsoleReceive",
                    Arguments = [level, message, thread]
                });
        });

    public Task SendTestAsync(
        string message,
        string thread,
        IHubCallerClients clients) =>
        TryCatch(operation: async () =>
        {
            ValidateOnSendTest(
                message: message,
                thread: thread,
                clients: clients);

            await notificationHubService.ExecuteNotificationHubOperationAsync(
                notificationHubOperation: new NotificationHubOperation
                {
                    Kind = NotificationHubOperationKind.SendToGroup,
                    Clients = clients,
                    Thread = thread,
                    Method = "ConsoleReceive",
                    Arguments = ["test", message, thread]
                });
        });
}