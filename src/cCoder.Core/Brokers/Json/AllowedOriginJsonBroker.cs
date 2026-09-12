// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.Json;

namespace cCoder.Core.Brokers.Json;

internal sealed class AllowedOriginJsonBroker : IAllowedOriginJsonBroker
{
    public IEnumerable<string> ExtractOrigins(string configJson) =>
        AllowedOriginJsonDependency.ExtractOrigins(configJson: configJson);
}