// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Packaging.Models;

namespace cCoder.Core.Services.Processings.Packages;

internal interface IPackageImportCompletionEventProcessingService
{
    ValueTask ProcessPackageImportEventAsync(
        PackageImportEvent packageImportEvent);
}