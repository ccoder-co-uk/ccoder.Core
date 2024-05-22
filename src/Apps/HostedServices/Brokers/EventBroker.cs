using EventLibrary;
using EventLibrary.Objects;
using HostedServices.Brokers.Interfaces;

namespace HostedServices.Brokers;

public class EventBroker(IEventHub eventHub)
    : IEventBroker
{
    public async ValueTask RaiseEventAsync<T>(string eventName, EventMessage<T> message) => await eventHub.RaiseEventAsync(eventName, message);
}