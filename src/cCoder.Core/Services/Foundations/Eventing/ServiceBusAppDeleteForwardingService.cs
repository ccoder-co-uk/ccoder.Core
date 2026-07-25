// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;
using cCoder.Eventing.AzureServiceBus.Models;
using cCoder.Eventing.Models;
using cCoder.Core.Brokers.Eventing;

namespace cCoder.Core.Services.Foundations.Eventing;

internal sealed partial class ServiceBusAppDeleteForwardingService(
    IServiceBusAppDeleteForwardingBroker forwardingBroker)
    : IServiceBusAppDeleteForwardingService
{
    public ValueTask ForwardAppDeleteAsync(App app) =>
        TryCatch(operation: async () =>
        {
            ValidateAppDeleteOnForward(app: app);

            ServiceBusEventMessage<App> message = new()
            {
                AuthInfo = new ServiceBusEventAuthInfo
                {
                    SSOUserId =
                        forwardingBroker.GetCurrentSsoUserId(),
                },
                Data = new App
                {
                    Id = app.Id,
                    Domain = app.Domain,
                    TenantId = app.TenantId,
                },
            };

            await forwardingBroker.RaiseAppDeleteEventAsync(
                message: message);
        });
}