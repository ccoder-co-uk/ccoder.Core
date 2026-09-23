// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Exposures;

namespace cCoder.Core.Brokers.Http;

internal sealed class HttpRequestBroker(IHttpContextAccessor httpContextAccessor)
    : IHttpRequestBroker, IUtilityBroker
{
    public HttpRequest GetCurrentRequest() =>
        httpContextAccessor.HttpContext?.Request;
}