// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Models;

namespace cCoder.Core.Brokers.Eventing;

internal interface IPackageImportCompletionEventBroker
{
    ValueTask RaisePackageImportEventCompleteAsync(
        PackageImportEvent packageImportEvent);
}