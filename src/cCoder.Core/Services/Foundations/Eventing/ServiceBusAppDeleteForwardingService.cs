using cCoder.Data.Models.CMS;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.AzureServiceBus.Models;
using cCoder.Security.Objects;

namespace cCoder.Core.Services.Foundations.Eventing;

internal class ServiceBusAppDeleteForwardingService(
    IAzureServiceBusEventHub serviceBusEventHub,
    ISSOAuthInfo authInfo)
{
    public async ValueTask ForwardAsync(App app)
    {
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
