// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Collections.Generic;
namespace cCoder.Core.Brokers.Json;

internal interface IAllowedOriginJsonBroker
{
    IEnumerable<string> ExtractOrigins(string configJson);
}