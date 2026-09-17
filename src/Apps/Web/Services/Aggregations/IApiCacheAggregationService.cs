// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Exposures;

namespace Web.Services.Aggregations;

public interface IApiCacheAggregationService : IApiCacheManager
{
    string GetMetadata(string culture);
}
