using Microsoft.AspNetCore.SignalR;

namespace cCoder.Core.Api.Hubs;

public abstract class CoreHub : Hub
{
    private readonly ILogger log;

    public struct HistoryItem
    {
        public string Level { get; set; }
        public string Message { get; set; }
    }

    private static readonly IDictionary<string, ICollection<HistoryItem>> History = new Dictionary<string, ICollection<HistoryItem>>();
    private static readonly IDictionary<string, int> UserCounts = new Dictionary<string, int>();

    public CoreHub(ILogger log)
    {
        this.log = log;
    }

    public override Task OnConnectedAsync()
    {
        log.LogDebug($"New Client connected to {GetType().Name}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        log.LogDebug($"Client disconnected from {GetType().Name}");
        return base.OnDisconnectedAsync(exception);
    }

    public virtual async Task Join(string thread)
    {
        log.LogDebug($"User joining {thread}");
        await Groups.AddToGroupAsync(Context.ConnectionId, thread);
        await Clients.Caller.SendAsync("ConsoleReceive", "info", "Connected to instance " + thread, thread);
        await Clients.Group(thread).SendAsync("ConsoleReceive", "info", "User Joined", thread);

        if (!History.ContainsKey(thread))
            History.Add(thread, new List<HistoryItem>());

        if (!UserCounts.ContainsKey(thread))
            UserCounts.Add(thread, 1);
        else
            UserCounts[thread]++;

        foreach (HistoryItem item in History[thread])
            await Clients.Caller.SendAsync("ConsoleReceive", item.Level, item.Message, thread);
    }

    public virtual async Task Leave(string thread)
    {
        log.LogDebug($"User leaving {thread}");

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, thread);
        await Clients.Caller.SendAsync("info", "Stopped listening to messages for " + thread, thread);
        await Clients.Group(thread).SendAsync("ConsoleReceive", "info", "User Left", thread);
        UserCounts[thread]--;

        if (UserCounts[thread] == 0)
            History.Remove(thread);
    }

    public virtual async Task ConsoleSend(string level, string message, string thread)
    {
        if (!History.ContainsKey(thread))
            History.Add(thread, new List<HistoryItem>());

        History[thread].Add(new HistoryItem { Message = message, Level = level });
        await Clients.Group(thread).SendAsync("ConsoleReceive", level, message, thread);
    }

    public virtual async Task SendTest(string message, string thread) =>
        await Clients.Group(thread).SendAsync("ConsoleReceive", "test", message, thread);
}