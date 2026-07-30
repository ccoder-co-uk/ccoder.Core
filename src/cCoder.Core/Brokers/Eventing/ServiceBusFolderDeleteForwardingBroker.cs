// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.DMS;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.AzureServiceBus.Models;
using cCoder.Security.Models.Configurations;

namespace cCoder.Core.Brokers.Eventing;

internal sealed class ServiceBusFolderDeleteForwardingBroker(
    IAzureServiceBusEventHub serviceBusEventHub,
    ISSOAuthInfo authInfo)
    : IServiceBusFolderDeleteForwardingBroker
{
    public string GetCurrentSsoUserId() =>
        authInfo.SSOUserId ?? string.Empty;

    public ValueTask RaiseFolderDeleteEventAsync(
        ServiceBusEventMessage<Folder> message) =>
        serviceBusEventHub.RaiseEventAsync(
            name: "folder_delete",
            message: message);
}