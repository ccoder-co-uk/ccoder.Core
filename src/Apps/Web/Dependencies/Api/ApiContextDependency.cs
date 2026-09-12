// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;

namespace Web.Dependencies.Api;

internal sealed class ApiContextDependency(IEnumerable<ApiInfo> apiInfos)
{
    public ApiInfo[] SelectAllApiInfos() =>
        [.. apiInfos];
}