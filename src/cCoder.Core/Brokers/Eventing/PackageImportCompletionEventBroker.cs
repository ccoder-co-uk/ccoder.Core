// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Models;
using cCoder.Eventing;
using cCoder.Eventing.Models;

namespace cCoder.Core.Brokers.Eventing;

internal sealed class PackageImportCompletionEventBroker(
    IEventHub eventHub)
    : IPackageImportCompletionEventBroker
{
    public ValueTask RaisePackageImportEventCompleteAsync(
        EventMessage<PackageImportEvent> message) =>
        eventHub.RaiseEventAsync(
            name: "package_import_complete",
            message: message);
}