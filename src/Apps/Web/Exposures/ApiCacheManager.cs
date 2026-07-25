// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Services.Aggregations;

namespace Web.Exposures;

internal sealed class ApiCacheManager(
    IApiCacheAggregationService apiCacheAggregationService)
    : IApiCacheManager
{
    public void RefreshCaches() =>
        apiCacheAggregationService.RefreshCaches();
}