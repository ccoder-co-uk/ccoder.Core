// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Models;
using cCoder.Core.Brokers.Eventing;
using cCoder.Eventing.Models;

namespace cCoder.Core.Services.Foundations.Eventing;

internal sealed partial class PackageImportCompletionEventService(
    IPackageImportCompletionEventBroker eventBroker,
    IAuthInfoBroker authInfoBroker)
    : IPackageImportCompletionEventService
{
    public ValueTask RaisePackageImportEventCompleteAsync(
        PackageImportEvent packageImportEvent) =>
        TryCatch(operation: async () =>
        {
            ValidatePackageImportEventOnRaise(
                packageImportEvent: packageImportEvent);

            await eventBroker.RaisePackageImportEventCompleteAsync(
                message: new EventMessage<PackageImportEvent>
                {
                    AuthInfo = CreateEventAuthInfo(),
                    Data = packageImportEvent,
                });
        });

    private EventAuthInfo CreateEventAuthInfo() =>
        new()
        {
            SSOUserId = authInfoBroker.GetCurrentSsoUserId(),
        };
}