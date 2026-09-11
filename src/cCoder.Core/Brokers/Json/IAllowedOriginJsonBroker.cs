// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Brokers.Json;

internal interface IAllowedOriginJsonBroker
{
    IEnumerable<string> ExtractOrigins(string configJson);
}