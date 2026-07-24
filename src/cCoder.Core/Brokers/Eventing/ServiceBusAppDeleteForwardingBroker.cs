// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.AzureServiceBus.Models;
using cCoder.Security.Objects;

namespace cCoder.Core.Brokers.Eventing;

internal sealed class ServiceBusAppDeleteForwardingBroker(
    IAzureServiceBusEventHub serviceBusEventHub,
    ISSOAuthInfo authInfo)
    : IServiceBusAppDeleteForwardingBroker
{
    public string GetCurrentSsoUserId() =>
        authInfo?.SSOUserId ?? string.Empty;

    public ValueTask RaiseAppDeleteEventAsync(
        ServiceBusEventMessage<App> message) =>
        serviceBusEventHub.RaiseEventAsync(
            name: "app_delete",
            message: message);
}