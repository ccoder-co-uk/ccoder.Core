namespace HostedServices.Logging;

public class SignalRLoggingProvider(IServiceProvider serviceProvider) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => serviceProvider.GetService<SignalRLoggingBroker>();

    public void Dispose() => GC.SuppressFinalize(this);
}