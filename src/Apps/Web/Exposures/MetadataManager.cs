// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Services.Aggregations;

namespace Web.Exposures;

internal sealed class MetadataManager(
    IApiCacheAggregationService apiCacheAggregationService)
    : IMetadataManager
{
    public string GetAll(string culture) =>
        apiCacheAggregationService.GetMetadata(culture: culture);

    public void LogError(Exception exception) =>
        apiCacheAggregationService.LogError(exception: exception);
}