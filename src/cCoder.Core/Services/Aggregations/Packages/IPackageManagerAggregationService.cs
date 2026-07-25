// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Aggregations.Packages;

public interface IPackageManagerAggregationService
{
    ValueTask<Package[]> ExportPackagesAsync(
        int appId,
        string[] packageNames,
        string sourceApi);

    ValueTask ImportPackagesAsync(
        int appId,
        IEnumerable<Package> packages);
}