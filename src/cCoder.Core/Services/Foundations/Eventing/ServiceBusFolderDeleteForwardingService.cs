using cCoder.Data.Models.DMS;
using cCoder.Eventing;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.AzureServiceBus.Models;
using cCoder.Eventing.Models;
using cCoder.Security.Objects;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.Core.Services.Foundations.Eventing;

internal class ServiceBusFolderDeleteForwardingService(
    IAzureServiceBusEventHub serviceBusEventHub,
    ISSOAuthInfo authInfo)
{
    public async ValueTask ForwardAsync(Folder folder)
    {
        await serviceBusEventHub.RaiseEventAsync(
            "folder_delete",
            new ServiceBusEventMessage<Folder>
            {
                AuthInfo = new ServiceBusEventAuthInfo
                {
                    SSOUserId = authInfo?.SSOUserId ?? string.Empty
                },
                Data = folder
            });
    }
}
