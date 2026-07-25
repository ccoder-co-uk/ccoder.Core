// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Eventing;
using cCoder.Data.Models.CMS;
using cCoder.Eventing.Models;

namespace cCoder.Core.Services.Foundations.Eventing;

internal sealed partial class AppGraphEventService(
    IAppGraphEventBroker appGraphEventBroker,
    IAuthInfoBroker authInfoBroker)
    : IAppGraphEventService
{
    public ValueTask RaiseAppAddEventAsync(App app) =>
        TryCatch(operation: async () =>
        {
            ValidateAppOnRaise(app: app);

            EventMessage<App> message =
                CreateAppEventMessage(app: app);

            await appGraphEventBroker.RaiseAppAddEventAsync(
                message: message);
        });

    public ValueTask RaiseAppUpdateEventAsync(App app) =>
        TryCatch(operation: async () =>
        {
            ValidateAppOnRaise(app: app);

            EventMessage<App> message =
                CreateAppEventMessage(app: app);

            await appGraphEventBroker.RaiseAppUpdateEventAsync(
                message: message);
        });

    private EventMessage<App> CreateAppEventMessage(App app) =>
        new()
        {
            AuthInfo = new EventAuthInfo
            {
                SSOUserId =
                    authInfoBroker.GetCurrentSsoUserId(),
            },
            Data = app,
        };
}