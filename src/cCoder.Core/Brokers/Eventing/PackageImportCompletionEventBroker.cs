// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Models;
using cCoder.Core.Dependencies.Eventing;

namespace cCoder.Core.Brokers.Eventing;

internal sealed class PackageImportCompletionEventBroker(
    EventingDependency eventingDependency)
    : IPackageImportCompletionEventBroker
{
    public ValueTask RaisePackageImportEventCompleteAsync(
        PackageImportEvent packageImportEvent) =>
        eventingDependency.RaiseEventAsync(
            name: "package_import_complete",
            data: packageImportEvent);
}