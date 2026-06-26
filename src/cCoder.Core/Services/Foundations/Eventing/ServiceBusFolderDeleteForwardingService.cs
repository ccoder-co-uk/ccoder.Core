using cCoder.Data.Models.DMS;
using cCoder.Eventing;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.AzureServiceBus.Models;
using cCoder.Eventing.Models;
using cCoder.Security.Objects;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.Core.Services.Foundations.Eventing;

internal class ServiceBusFolderDeleteForwardingService(
    IServiceProvider serviceProvider,
    ISSOAuthInfo authInfo)
{
    public async ValueTask ForwardAsync(Folder folder)
    {
        IAzureServiceBusEventHub serviceBusEventHub =
            serviceProvider.GetService<IAzureServiceBusEventHub>();

        if (serviceBusEventHub is null)
        {
            IEventHub eventHub = serviceProvider.GetRequiredService<IEventHub>();
            await eventHub.RaiseEventAsync(
                "folder_delete",
                new EventMessage<Folder>
                {
                    AuthInfo = new EventAuthInfo
                    {
                        SSOUserId = authInfo?.SSOUserId ?? string.Empty
                    },
                    Data = folder
                });

            return;
        }

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
