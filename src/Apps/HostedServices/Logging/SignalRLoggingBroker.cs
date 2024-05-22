using Microsoft.AspNetCore.SignalR;

namespace HostedServices.Logging;

public class SignalRLoggingBroker(IServiceProvider services) : ILogger
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public async void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        IHubContext<LogHub> hubContext = services.GetService<IHubContext<LogHub>>();

        if (hubContext is not null)
        {
            await hubContext?
                .Clients?
                .All?
                .SendAsync("ConsoleReceive", logLevel.ToString(), formatter(state, exception));
        }
    }
}