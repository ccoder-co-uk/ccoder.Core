// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using ContentManagementCommonObjectCache =
    cCoder.ContentManagement.Exposures.Caching.ICommonObjectCache;
using ContentManagementMetadataCache =
    cCoder.ContentManagement.Exposures.Caching.IMetadataCache;

namespace Web.Services.Aggregations;

internal sealed partial class ApiCacheAggregationService(
    ContentManagementCommonObjectCache commonObjectCache,
    ContentManagementMetadataCache metadataCache)
    : IApiCacheAggregationService
{
    public void RefreshCaches() =>
        TryCatch(operation: () =>
        {
            ValidateCachesOnRefresh();

            commonObjectCache.Refresh();
            metadataCache.Rebuild();
        });
}