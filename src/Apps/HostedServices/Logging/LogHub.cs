using cCoder.Data.Models.Security;
using cCoder.Logging.Brokers;
using Microsoft.AspNetCore.SignalR;


namespace HostedServices.Logging;

public class LogHub(IAuthorizationBroker authorizationBroker, ILogger<LogHub> log)
    : BaseHub(log)
{
    public override async Task Join(string thread)
    {
        log.LogDebug($"User joining {thread}");
        await Groups.AddToGroupAsync(Context.ConnectionId, thread);
        await Clients.Caller.SendAsync("ConsoleReceive", "info", "Connected to instance " + thread, thread);
        await Clients.Group(thread).SendAsync("ConsoleReceive", "info", "User Joined", thread);

        User user = authorizationBroker.GetCurrentUser();
        log.LogInformation($"User {user.Id} is listening to log stream for domain {thread}");
    }

    public override async Task Leave(string thread)
    {
        log.LogDebug($"User leaving {thread}");

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, thread);
        await Clients.Caller.SendAsync("info", "Stopped listening to messages for " + thread, thread);
        await Clients.Group(thread).SendAsync("ConsoleReceive", "info", "User Left", thread);

        User user = authorizationBroker.GetCurrentUser();
        log.LogInformation($"User {user.Id} stopped listening to log stream for domain {thread}");
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        User user = authorizationBroker.GetCurrentUser();
        log.LogInformation($"User {user.Id} disconnected.");
        return Task.CompletedTask;
    }

    public void Debug(string level, string message) => log.LogDebug($"{Context.GetHttpContext().Request.Host.Value}: {level} {message}");

    public void Info(string level, string message) => log.LogInformation($"{Context.GetHttpContext().Request.Host.Value}: {level} {message}");

    public void Warn(string level, string message) => log.LogWarning($"{Context.GetHttpContext().Request.Host.Value}: {level} {message}");

    public void Error(string level, string message) => log.LogError($"{Context.GetHttpContext().Request.Host.Value}: {level} {message}");

    public override async Task ConsoleSend(string level, string message, string thread) => await base.ConsoleSend(level, message, thread);
}






