// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;
using cCoder.Eventing;
using cCoder.Eventing.Models;

namespace cCoder.Core.Brokers.Eventing;

internal sealed class AppGraphEventBroker(
    IEventHub eventHub)
    : IAppGraphEventBroker
{
    public ValueTask RaiseAppAddEventAsync(EventMessage<App> message) =>
        eventHub.RaiseEventAsync(
            name: "app_add",
            message: message);

    public ValueTask RaiseAppUpdateEventAsync(EventMessage<App> message) =>
        eventHub.RaiseEventAsync(
            name: "app_update",
            message: message);
}