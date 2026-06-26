namespace cCoder.Core.Brokers.Http;

internal sealed class HttpRequestBroker(IHttpContextAccessor httpContextAccessor) : IHttpRequestBroker
{
    public HttpRequest GetCurrentRequest() =>
        httpContextAccessor.HttpContext?.Request;
}
