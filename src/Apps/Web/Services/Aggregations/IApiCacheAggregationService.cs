// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
namespace Web.Services.Aggregations;

public interface IApiCacheAggregationService
{
    void RefreshCaches();
    string GetMetadata(string culture);
    void LogError(Exception exception);
}