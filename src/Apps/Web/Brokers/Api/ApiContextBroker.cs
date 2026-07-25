// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;

namespace Web.Brokers.Api;

internal sealed class ApiContextBroker(
    IEnumerable<ApiInfo> apiInfos)
    : IApiContextBroker
{
    public ApiInfo[] SelectAllApiInfos() =>
        apiInfos.ToArray();
}