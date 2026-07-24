// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;
using cCoder.Core.Dependencies.Eventing;
using cCoder.Eventing.AzureServiceBus.Models;

namespace cCoder.Core.Brokers.Eventing;

internal sealed class ServiceBusAppDeleteForwardingBroker(
    ServiceBusEventingDependency serviceBusEventingDependency)
    : IServiceBusAppDeleteForwardingBroker
{
    public string GetCurrentSsoUserId() =>
        serviceBusEventingDependency.GetCurrentSsoUserId();

    public ValueTask RaiseAppDeleteEventAsync(
        ServiceBusEventMessage<App> message) =>
        serviceBusEventingDependency.RaiseEventAsync(
            name: "app_delete",
            message: message);
}