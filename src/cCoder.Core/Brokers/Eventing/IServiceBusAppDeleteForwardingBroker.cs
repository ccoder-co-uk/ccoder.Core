// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;
using cCoder.Eventing.AzureServiceBus.Models;

namespace cCoder.Core.Brokers.Eventing;

internal interface IServiceBusAppDeleteForwardingBroker
{
    string GetCurrentSsoUserId();

    ValueTask RaiseAppDeleteEventAsync(
        ServiceBusEventMessage<App> message);
}