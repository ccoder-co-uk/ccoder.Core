// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;
using cCoder.Core.Dependencies.Eventing;

namespace cCoder.Core.Brokers.Eventing;

internal sealed class AppGraphEventBroker(
    EventingDependency eventingDependency)
    : IAppGraphEventBroker
{
    public ValueTask RaiseAppAddEventAsync(App app) =>
        eventingDependency.RaiseEventAsync(
            name: "app_add",
            data: app);

    public ValueTask RaiseAppUpdateEventAsync(App app) =>
        eventingDependency.RaiseEventAsync(
            name: "app_update",
            data: app);
}