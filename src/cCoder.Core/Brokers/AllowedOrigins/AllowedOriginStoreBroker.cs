// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.AllowedOrigins;

namespace cCoder.Core.Brokers.AllowedOrigins;

internal sealed class AllowedOriginStoreBroker(
    AllowedOriginStoreDependency allowedOriginStoreDependency)
    : IAllowedOriginStoreBroker
{
    public IEnumerable<string> GetAllowedOrigins() =>
        allowedOriginStoreDependency.GetAllowedOrigins();
}