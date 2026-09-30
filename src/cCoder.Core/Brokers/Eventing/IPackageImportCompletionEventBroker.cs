// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;
using cCoder.ContentManagement.Models;

namespace cCoder.Core.Brokers.Eventing;

internal interface IPackageImportCompletionEventBroker
{
    ValueTask RaisePackageImportEventCompleteAsync(
        PackageImportEvent packageImportEvent);
}