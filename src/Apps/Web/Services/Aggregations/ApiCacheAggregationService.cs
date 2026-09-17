// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Loggings;
using Web.Brokers.Api;
using Web.Exposures;

namespace Web.Services.Aggregations;

internal sealed partial class ApiCacheAggregationService(
    ICommonObjectCacheBroker commonObjectCacheBroker,
    IMetadataCacheBroker metadataCacheBroker,
    ILoggingBroker loggingBroker)
    : IApiCacheAggregationService
{
    public void RefreshCaches() =>
        TryCatch(operation: () =>
        {
            ValidateCaches();

            commonObjectCacheBroker.Refresh();
            metadataCacheBroker.Rebuild();
        });

    public string GetMetadata(string culture) =>
        TryCatch(operation: () =>
        {
            ValidateMetadataOnGet(culture: culture);

            return metadataCacheBroker.GetAll(culture: culture);
        });

    void IApiCacheManager.LogError(Exception exception) =>
        loggingBroker.LogError(
            exception: exception,
            message: "HTTP request failed.");
}