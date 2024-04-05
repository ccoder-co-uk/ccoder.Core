using Microsoft.AspNetCore.SignalR;

namespace cCoder.Core.Api.Hubs
{
    public class NotificationHub : CoreHub
    {
        public NotificationHub(ILogger<NotificationHub> log) : base(log) { }

        public void Send(string level, string message, string thread) =>
            // sends a normal notification message
            Clients.Group(thread).SendAsync(level, message);

        public override async Task ConsoleSend(string level, string message, string thread) =>
            await Clients.Group(thread).SendAsync("ConsoleReceive", level, message, thread);
    }
}