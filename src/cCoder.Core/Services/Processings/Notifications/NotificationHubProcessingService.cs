// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Loggings;
using cCoder.Core.Models.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace cCoder.Core.Services.Processings.Notifications;

internal sealed partial class NotificationHubProcessingService(
    ILoggingBroker log)
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

            await groups.AddToGroupAsync(
                connectionId: connectionId,
                groupName: thread);

            await clients.Caller.SendAsync(
                method: "ConsoleReceive",
                arg1: "info",
                arg2: "Connected to instance " + thread,
                arg3: thread);

            await clients.Group(groupName: thread)
                .SendAsync(
                    method: "ConsoleReceive",
                    arg1: "info",
                    arg2: "User Joined",
                    arg3: thread);

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
                await clients.Caller.SendAsync(
                    method: "ConsoleReceive",
                    arg1: item.Level,
                    arg2: item.Message,
                    arg3: thread);
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

            await groups.RemoveFromGroupAsync(
                connectionId: connectionId,
                groupName: thread);

            await clients.Caller.SendAsync(
                method: "info",
                arg1: "Stopped listening to messages for " + thread,
                arg2: thread);

            await clients.Group(groupName: thread)
                .SendAsync(
                    method: "ConsoleReceive",
                    arg1: "info",
                    arg2: "User Left",
                    arg3: thread);

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

            clients.Group(groupName: thread)
                .SendAsync(method: level, arg1: message);
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

            await clients.Group(groupName: thread)
                .SendAsync(
                    method: "ConsoleReceive",
                    arg1: level,
                    arg2: message,
                    arg3: thread);
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

            await clients.Group(groupName: thread)
                .SendAsync(
                    method: "ConsoleReceive",
                    arg1: "test",
                    arg2: message,
                    arg3: thread);
        });
}