using cCoder.Core.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Web.Logging
{
    public class SignalRLoggingBroker : ILogger
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public SignalRLoggingBroker(IHttpContextAccessor httpContextAccessor) =>
            this.httpContextAccessor = httpContextAccessor;

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public async void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                var thread = httpContextAccessor?.HttpContext?.Request?.Host.Value.Split(":")[0];

                if (thread is not null)
                {
                    var hubContext = httpContextAccessor.HttpContext.RequestServices
                        .GetService<IHubContext<LogHub>>();

                    await hubContext?.Clients
                        .Group(thread)
                        .SendAsync("ConsoleReceive", logLevel.ToString(), formatter(state, exception));
                }
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.ToString());
            }
        }
    }
}