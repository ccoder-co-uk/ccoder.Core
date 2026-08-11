// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Packaging.Models;

namespace cCoder.Core.Services.Aggregations.Packages;

internal sealed partial class PackageImportAggregationService
{
    private static void ValidatePackageImportEvent(
        PackageImportEvent packageImportEvent) =>
        ValidationRulesEngine.Validate(inputs: [packageImportEvent]);
}