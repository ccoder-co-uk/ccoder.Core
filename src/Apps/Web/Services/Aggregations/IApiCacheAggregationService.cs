// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Services.Aggregations;

internal interface IApiCacheAggregationService
{
    void RefreshCaches();
    string GetMetadata(string culture);
    void LogError(Exception exception);
}