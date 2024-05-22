using EventLibrary.Objects;

namespace HostedServices.Brokers.Interfaces;

public interface IEventBroker
{
    ValueTask RaiseEventAsync<T>(string eventName, EventMessage<T> message);
}
