// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using ContentManagementCommonObjectCache =
    cCoder.ContentManagement.Exposures.Caching.ICommonObjectCache;

namespace Web.Brokers.Api;

internal sealed class CommonObjectCacheBroker(
    ContentManagementCommonObjectCache commonObjectCache)
    : ICommonObjectCacheBroker
{
    public void Refresh() =>
        commonObjectCache.Refresh();
}