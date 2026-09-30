// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;
using cCoder.Packaging.Models;

namespace cCoder.Core.Services.Aggregations.Packages;

internal interface IPackageImportAggregationService
{
    ValueTask ProcessPackageImportEventAsync(
        PackageImportEvent packageImportEvent);
}