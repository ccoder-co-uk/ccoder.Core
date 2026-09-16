// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Brokers.Json;

internal sealed class AllowedOriginJsonBroker : IAllowedOriginJsonBroker
{
    public IEnumerable<string> ExtractOrigins(string configJson) =>
        AllowedOriginJsonParser.ExtractOrigins(configJson: configJson);
}