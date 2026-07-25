// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Eventing.AzureServiceBus;

namespace cCoder.Core.Brokers.Eventing;

internal interface IServiceBusEventingBroker
    : IAzureServiceBusEventHub
{
    string GetCurrentSsoUserId();
}