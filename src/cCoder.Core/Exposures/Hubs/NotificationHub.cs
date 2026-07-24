// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.SignalR;


namespace cCoder.Core.Exposures.Hubs;

public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> log;
    private static readonly IDictionary<string, ICollection<HistoryItem>> History =
        new Dictionary<string, ICollection<HistoryItem>>();
    private static readonly IDictionary<string, int> UserCounts = new Dictionary<string, int>();

    public NotificationHub(ILogger<NotificationHub> log) => this.log = log;

    public struct HistoryItem
    {
        public string Level { get; set; }
        public string Message { get; set; }
    }

    public override Task OnConnectedAsync()
    {
        log.LogDebug(message: $"New Client connected to {GetType().Name}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        log.LogDebug(message: $"Client disconnected from {GetType().Name}");
        return base.OnDisconnectedAsync(exception: exception);
    }

    public async Task Join(string thread)
    {
        log.LogDebug(message: $"User joining {thread}");
        await Groups.AddToGroupAsync(connectionId: Context.ConnectionId, groupName: thread);
        await Clients.Caller.SendAsync(
method: "ConsoleReceive", arg1: "info", arg2: "Connected to instance " + thread, arg3: thread
        );
        await Clients.Group(groupName: thread)
            .SendAsync(method: "ConsoleReceive", arg1: "info", arg2: "User Joined", arg3: thread);

        if (!History.ContainsKey(key: thread))
        {
            History.Add(key: thread, value: new List<HistoryItem>());
        }

        if (!UserCounts.ContainsKey(key: thread))
        {
            UserCounts.Add(key: thread, value: 1);
        }
        else
        {
            UserCounts[thread]++;
        }

        foreach (HistoryItem item in History[thread])
        {
            await Clients.Caller.SendAsync(method: "ConsoleReceive", arg1: item.Level, arg2: item.Message, arg3: thread);
        }
    }

    public async Task Leave(string thread)
    {
        log.LogDebug(message: $"User leaving {thread}");

        await Groups.RemoveFromGroupAsync(connectionId: Context.ConnectionId, groupName: thread);
        await Clients.Caller.SendAsync(
method: "info", arg1: "Stopped listening to messages for " + thread, arg2: thread
        );
        await Clients.Group(groupName: thread)
            .SendAsync(method: "ConsoleReceive", arg1: "info", arg2: "User Left", arg3: thread);
        UserCounts[thread]--;

        if (UserCounts[thread] == 0)
        {
            History.Remove(key: thread);
        }
    }

    public void Send(string level, string message, string thread) =>
        // sends a normal notification message
        Clients.Group(groupName: thread)
            .SendAsync(method: level, arg1: message);

    public async Task ConsoleSend(string level, string message, string thread)
    {
        if (!History.ContainsKey(key: thread))
        {
            History.Add(key: thread, value: new List<HistoryItem>());
        }

        History[thread].Add(item: new HistoryItem { Message = message, Level = level });
        await Clients.Group(groupName: thread)
            .SendAsync(method: "ConsoleReceive", arg1: level, arg2: message, arg3: thread);
    }

    public async Task SendTest(string message, string thread) =>
        await Clients.Group(groupName: thread)
            .SendAsync(method: "ConsoleReceive", arg1: "test", arg2: message, arg3: thread);
}