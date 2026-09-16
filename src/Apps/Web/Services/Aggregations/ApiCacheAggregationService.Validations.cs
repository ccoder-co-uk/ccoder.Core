// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Services.Aggregations;

internal sealed partial class ApiCacheAggregationService
{
    private static void Validate(params object[] inputs)
    {
        if (inputs.Any(predicate: input => input is null))
        {
            throw new ArgumentNullException(paramName: nameof(inputs));
        }
    }

    private static void ValidateCaches()
    {
    }

    private static void ValidateMetadataOnGet(string culture) =>
        Validate(inputs: culture);
}