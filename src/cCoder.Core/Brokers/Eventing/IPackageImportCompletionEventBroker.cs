// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Models;
using cCoder.Eventing.Models;

namespace cCoder.Core.Brokers.Eventing;

internal interface IPackageImportCompletionEventBroker
{
    ValueTask RaisePackageImportEventCompleteAsync(
        EventMessage<PackageImportEvent> message);
}