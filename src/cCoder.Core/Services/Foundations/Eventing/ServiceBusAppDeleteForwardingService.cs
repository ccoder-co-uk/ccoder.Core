using cCoder.Data.Models.CMS;
using cCoder.Eventing;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.AzureServiceBus.Models;
using cCoder.Eventing.Models;
using cCoder.Security.Objects;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.Core.Services.Foundations.Eventing;

internal class ServiceBusAppDeleteForwardingService(
    IServiceProvider serviceProvider,
    ISSOAuthInfo authInfo)
{
    public async ValueTask ForwardAsync(App app)
    {
        IAzureServiceBusEventHub serviceBusEventHub =
            serviceProvider.GetService<IAzureServiceBusEventHub>();

        if (serviceBusEventHub is null)
        {
            IEventHub eventHub = serviceProvider.GetRequiredService<IEventHub>();
            await eventHub.RaiseEventAsync(
                "app_delete",
                new EventMessage<App>
                {
                    AuthInfo = new EventAuthInfo
                    {
                        SSOUserId = authInfo?.SSOUserId ?? string.Empty
                    },
                    Data = new App
                    {
                        Id = app.Id,
                        Domain = app.Domain,
                        TenantId = app.TenantId
                    }
                });

            return;
        }

        await serviceBusEventHub.RaiseEventAsync(
            "app_delete",
            new ServiceBusEventMessage<App>
            {
                AuthInfo = new ServiceBusEventAuthInfo
                {
                    SSOUserId = authInfo?.SSOUserId ?? string.Empty
                },
                Data = new App
                {
                    Id = app.Id,
                    Domain = app.Domain,
                    TenantId = app.TenantId
                }
            });
    }
}
