using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using Microsoft.AspNetCore.SignalR;

namespace cCoder.Core.Api.Hubs;

public class LogHub : CoreHub
{
    private readonly ILogger log;
    private readonly ICoreDataContext db;

    public LogHub(ICoreDataContext db, ILogger<LogHub> log) : base(log) 
    {
        this.db = db; 
        this.log = log;
    }

    public override async Task Join(string thread)
    {
        log.LogDebug($"User joining {thread}");

        int? app = db.GetAll<App>()
            .FirstOrDefault(a => a.Domain == thread)?
            .Id;

        if (db.User.IsAdminOfApp(app ?? 0))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, thread);
            await Clients.Caller.SendAsync("ConsoleReceive", "info", "Connected to instance " + thread, thread);
            await Clients.Group(thread).SendAsync("ConsoleReceive", "info", "User Joined", thread);
            log.LogInformation($"User {db.User.Id} is listening to log stream for domain {thread}");
        }
        else
            log.LogWarning($"User {db.User.Id} denied access to log stream for domain {thread}");
    }

    public override async Task Leave(string thread)
    {
        log.LogDebug($"User leaving {thread}");

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, thread);
        await Clients.Caller.SendAsync("info", "Stopped listening to messages for " + thread, thread);
        await Clients.Group(thread).SendAsync("ConsoleReceive", "info", "User Left", thread);
        log.LogInformation($"User {db.User.Id} stopped listening to log stream for domain {thread}");
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        log.LogInformation($"User {db.User.Id} disconnected.");
        return Task.CompletedTask;
    }

    public void Debug(string level, string message) => 
        log.LogDebug($"{Context.GetHttpContext().Request.Host.Value}: {level} {message}");

    public void Info(string level, string message) => 
        log.LogInformation($"{Context.GetHttpContext().Request.Host.Value}: {level} {message}");

    public void Warn(string level, string message) => 
        log.LogWarning($"{Context.GetHttpContext().Request.Host.Value}: {level} {message}");

    public void Error(string level, string message) => 
        log.LogError($"{Context.GetHttpContext().Request.Host.Value}: {level} {message}");

    public override async Task ConsoleSend(string level, string message, string thread) =>
        await base.ConsoleSend(level, message, thread);
}