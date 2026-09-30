// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;
using cCoder.Data.Models.DMS;
using cCoder.Eventing.AzureServiceBus.Models;

namespace cCoder.Core.Brokers.Eventing;

internal interface IServiceBusFolderDeleteForwardingBroker
{
    string GetCurrentSsoUserId();

    ValueTask RaiseFolderDeleteEventAsync(
        ServiceBusEventMessage<Folder> message);
}