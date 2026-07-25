// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.DMS;
using cCoder.Eventing.AzureServiceBus.Models;
using cCoder.Eventing.Models;
using cCoder.Core.Brokers.Eventing;

namespace cCoder.Core.Services.Foundations.Eventing;

internal sealed partial class ServiceBusFolderDeleteForwardingService(
    IServiceBusFolderDeleteForwardingBroker forwardingBroker)
    : IServiceBusFolderDeleteForwardingService
{
    public ValueTask ForwardFolderDeleteAsync(Folder folder) =>
        TryCatch(operation: async () =>
        {
            ValidateFolderDeleteOnForward(folder: folder);

            ServiceBusEventMessage<Folder> message = new()
            {
                AuthInfo = new ServiceBusEventAuthInfo
                {
                    SSOUserId =
                        forwardingBroker.GetCurrentSsoUserId(),
                },
                Data = folder,
            };

            await forwardingBroker.RaiseFolderDeleteEventAsync(
                message: message);
        });
}