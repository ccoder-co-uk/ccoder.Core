// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;

namespace cCoder.Core.Services.Aggregations.Packages;

internal sealed partial class PackageManagerAggregationService
{
    private static void ValidatePackagesOnExport(
        int appId,
        string[] packageNames,
        string sourceApi)
    {
        ValidationRulesEngine.Validate(inputs: [appId, sourceApi]);

        if (packageNames is not null)
        {
            ValidationRulesEngine.Validate(inputs: [packageNames]);
        }
    }
}