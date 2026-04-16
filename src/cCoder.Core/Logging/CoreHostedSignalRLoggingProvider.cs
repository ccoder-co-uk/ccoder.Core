using cCoder.Logging.Exposures.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace cCoder.Core.Logging;

internal sealed class CoreHostedSignalRLoggingProvider(IServiceProvider serviceProvider) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) =>
        new CoreHostedSignalRLogger(serviceProvider, categoryName);

    public void Dispose() => GC.SuppressFinalize(this);
}

internal sealed class CoreHostedSignalRLogger(
    IServiceProvider serviceProvider,
    string categoryName) : ILogger
{
    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public async void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        try
        {
            if (ShouldIgnoreCategory(categoryName))
                return;

            IHubContext<LogHub> hubContext = serviceProvider.GetService<IHubContext<LogHub>>();

            if (hubContext is null)
                return;

            await hubContext.Clients
                .All
                .SendAsync("ConsoleReceive", logLevel.ToString(), formatter(state, exception));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static bool ShouldIgnoreCategory(string categoryName) =>
        categoryName.StartsWith("Microsoft.AspNetCore.SignalR", StringComparison.Ordinal)
        || categoryName.StartsWith("Microsoft.AspNetCore.Http.Connections", StringComparison.Ordinal)
        || categoryName.StartsWith("System.Net.Http.HttpClient", StringComparison.Ordinal)
        || categoryName == typeof(LogHub).FullName
        || categoryName == typeof(CoreHostedSignalRLogger).FullName;
}
