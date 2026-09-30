// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;
using cCoder.Packaging.Models;

namespace cCoder.Core.Services.Processings.Packages;

internal interface IPackageImportCompletionEventProcessingService
{
    ValueTask ProcessPackageImportEventAsync(
        PackageImportEvent packageImportEvent);
}