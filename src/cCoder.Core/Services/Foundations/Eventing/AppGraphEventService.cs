// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Eventing;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.Eventing;

internal sealed partial class AppGraphEventService(
    IAppGraphEventBroker appGraphEventBroker)
    : IAppGraphEventService
{
    public ValueTask RaiseAppAddEventAsync(App app) =>
        TryCatch(operation: async () =>
        {
            ValidateAppOnRaise(app: app);

            await appGraphEventBroker.RaiseAppAddEventAsync(
                app: app);
        });

    public ValueTask RaiseAppUpdateEventAsync(App app) =>
        TryCatch(operation: async () =>
        {
            ValidateAppOnRaise(app: app);

            await appGraphEventBroker.RaiseAppUpdateEventAsync(
                app: app);
        });
}