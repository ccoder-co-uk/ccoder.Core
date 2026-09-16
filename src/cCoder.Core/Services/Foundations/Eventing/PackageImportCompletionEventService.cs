// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Models;
using cCoder.Core.Brokers.Eventing;

namespace cCoder.Core.Services.Foundations.Eventing;

internal sealed partial class PackageImportCompletionEventService(
    IPackageImportCompletionEventBroker eventBroker)
    : IPackageImportCompletionEventService
{
    public ValueTask RaisePackageImportEventCompleteAsync(
        PackageImportEvent packageImportEvent) =>
        TryCatch(operation: async () =>
        {
            ValidatePackageImportEventOnRaise(
                packageImportEvent: packageImportEvent);

            await eventBroker.RaisePackageImportEventCompleteAsync(
                packageImportEvent: packageImportEvent);
        });
}