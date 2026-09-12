// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;
using Web.Dependencies.Api;

namespace Web.Brokers.Api;

internal sealed class ApiContextBroker(
    ApiContextDependency apiContextDependency)
    : IApiContextBroker
{
    public ApiInfo[] SelectAllApiInfos() =>
        apiContextDependency.SelectAllApiInfos();
}