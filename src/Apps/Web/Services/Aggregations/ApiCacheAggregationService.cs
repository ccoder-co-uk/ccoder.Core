// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Brokers.Api;

namespace Web.Services.Aggregations;

internal sealed partial class ApiCacheAggregationService(
    ICommonObjectCacheBroker commonObjectCacheBroker,
    IMetadataCacheBroker metadataCacheBroker)
    : IApiCacheAggregationService
{
    public void RefreshCaches() =>
        TryCatch(operation: () =>
        {
            ValidateCachesOnRefresh();

            commonObjectCacheBroker.Refresh();
            metadataCacheBroker.Rebuild();
        });
}