// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Models;

namespace cCoder.Core.Services.Foundations.Eventing;

internal interface IPackageImportCompletionEventService
{
    ValueTask RaisePackageImportEventCompleteAsync(
        PackageImportEvent packageImportEvent);
}