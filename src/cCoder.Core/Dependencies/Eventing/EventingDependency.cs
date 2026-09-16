// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Eventing;
using cCoder.Eventing.Models;

namespace cCoder.Core.Dependencies.Eventing;

internal sealed class EventingDependency(
    IEventHub eventHub,
    ICoreAuthInfo authInfo)
    : IEventHub
{
    public void ListenToEvent<T, TService>(
        string name,
        Func<TService, T, ValueTask> handler) =>
        eventHub.ListenToEvent(
            name: name,
            handler: handler);

    public ValueTask RaiseEventAsync<T>(
        string name,
        EventMessage<T> message) =>
        eventHub.RaiseEventAsync(
            name: name,
            message: message);

    public ValueTask RaiseEventsAsync<T>(
        string name,
        EventMessage<T>[] messages) =>
        eventHub.RaiseEventsAsync(
            name: name,
            messages: messages);

    internal ValueTask RaiseEventAsync<T>(
        string name,
        T data) =>
        eventHub.RaiseEventAsync(
            name: name,
            message: new EventMessage<T>
            {
                AuthInfo = new EventAuthInfo
                {
                    SSOUserId = authInfo.SSOUserId,
                },
                Data = data,
            });
}