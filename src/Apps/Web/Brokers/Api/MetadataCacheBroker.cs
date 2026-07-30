// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using ContentManagementMetadataCache =
    cCoder.ContentManagement.Exposures.Caching.IMetadataCache;

namespace Web.Brokers.Api;

internal sealed class MetadataCacheBroker(
    ContentManagementMetadataCache metadataCache)
    : IMetadataCacheBroker
{
    public void Rebuild() =>
        metadataCache.Rebuild();
}