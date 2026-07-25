// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.DMS;
using cCoder.Core.Dependencies.Eventing;
using cCoder.Eventing.AzureServiceBus.Models;

namespace cCoder.Core.Brokers.Eventing;

internal sealed class ServiceBusFolderDeleteForwardingBroker(
    IServiceBusEventingBroker serviceBusEventingDependency)
    : IServiceBusFolderDeleteForwardingBroker
{
    public string GetCurrentSsoUserId() =>
        serviceBusEventingDependency.GetCurrentSsoUserId();

    public ValueTask RaiseFolderDeleteEventAsync(
        ServiceBusEventMessage<Folder> message) =>
        serviceBusEventingDependency.RaiseEventAsync(
            name: "folder_delete",
            message: message);
}