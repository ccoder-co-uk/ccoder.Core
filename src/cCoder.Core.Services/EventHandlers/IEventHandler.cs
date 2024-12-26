namespace cCoder.Core.Services.EventHandlers
{
    public interface IEvent<T>
    {
        public T Subject { get; set; }
    }

    public interface IEventHandler<T, T2> where T : IEvent<T2>
        where T2 : class
    {
        Task HandleAsync(T @event);
    }
}
