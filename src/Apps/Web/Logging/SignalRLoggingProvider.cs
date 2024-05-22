namespace Web.Logging
{
    public class SignalRLoggingProvider : ILoggerProvider
    {
        private readonly IServiceProvider serviceProvider;

        public SignalRLoggingProvider(IServiceProvider serviceProvider) =>
            this.serviceProvider = serviceProvider;

        public ILogger CreateLogger(string categoryName) =>
            serviceProvider.GetService<SignalRLoggingBroker>();

        public void Dispose() { }
    }
}