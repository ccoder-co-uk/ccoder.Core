using cCoder.Core.Services.EventHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.Core.Services
{
    public class EventService(IServiceProvider provider) : IEventService
    {
        public async Task RaiseEventAsync<T, T2>(T ev) 
            where T : IEvent<T2>
            where T2 : class
        {
            var applicableServices = provider.GetServices<IEventHandler<T, T2>>();

            foreach (var entry in applicableServices)
                await entry.HandleAsync(ev);
        }
    }
}
