// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.AzureServiceBus.Models;
using cCoder.Core.Brokers.Eventing;
using cCoder.Security.Objects;

namespace cCoder.Core.Dependencies.Eventing;

internal sealed class ServiceBusEventingDependency(
    IAzureServiceBusEventHub serviceBusEventHub,
    ISSOAuthInfo authInfo)
    : IServiceBusEventingBroker,
      IAzureServiceBusEventHub
{
    public string GetCurrentSsoUserId() =>
        authInfo?.SSOUserId ?? string.Empty;

    public void ListenToEvent<T>(
        string name,
        Func<IServiceProvider, T, ValueTask> handler) =>
        serviceBusEventHub.ListenToEvent(
            name: name,
            handler: handler);

    public ValueTask RaiseEventAsync<T>(
        string name,
        ServiceBusEventMessage<T> message) =>
        serviceBusEventHub.RaiseEventAsync(
            name: name,
            message: message);

    public ValueTask RaiseEventsAsync<T>(
        string name,
        ServiceBusEventMessage<T>[] messages) =>
        serviceBusEventHub.RaiseEventsAsync(
            name: name,
            messages: messages);
}