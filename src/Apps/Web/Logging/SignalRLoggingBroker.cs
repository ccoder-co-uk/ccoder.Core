using cCoder.Logging.Exposures.Hubs;
using Microsoft.AspNetCore.SignalR;


namespace Web.Logging
{
    public class SignalRLoggingBroker : ILogger
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IServiceProvider serviceProvider;
        private readonly string categoryName;

        public SignalRLoggingBroker(
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider,
            string categoryName = "")
        {
            this.httpContextAccessor = httpContextAccessor;
            this.serviceProvider = serviceProvider;
            this.categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public async void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                if (ShouldIgnoreCategory())
                    return;

                string thread = httpContextAccessor?.HttpContext?.Request?.Host.Value.Split(":")[0];

                if (string.IsNullOrWhiteSpace(thread))
                    return;

                IHubContext<LogHub> hubContext =
                    serviceProvider.GetService<IHubContext<LogHub>>();

                if (hubContext is null)
                    return;

                await hubContext.Clients
                    .Group(thread)
                    .SendAsync("ConsoleReceive", logLevel.ToString().ToLowerInvariant(), formatter(state, exception), thread);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private bool ShouldIgnoreCategory() =>
            categoryName.StartsWith("Microsoft.AspNetCore.SignalR", StringComparison.Ordinal)
            || categoryName.StartsWith("Microsoft.AspNetCore.Http.Connections", StringComparison.Ordinal)
            || categoryName.StartsWith("System.Net.Http.HttpClient", StringComparison.Ordinal)
            || categoryName == typeof(LogHub).FullName
            || categoryName == typeof(SignalRLoggingBroker).FullName;
    }
}





