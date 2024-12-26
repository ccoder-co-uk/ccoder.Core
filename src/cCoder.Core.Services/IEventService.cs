using cCoder.Core.Services.EventHandlers;

namespace cCoder.Core.Services
{
    public interface IEventService
    {
        Task RaiseEventAsync<T, T2>(T ev)
            where T : IEvent<T2>
            where T2 : class;
    }
}