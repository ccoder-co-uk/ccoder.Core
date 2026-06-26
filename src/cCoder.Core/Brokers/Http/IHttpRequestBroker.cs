namespace cCoder.Core.Brokers.Http;

internal interface IHttpRequestBroker
{
    HttpRequest GetCurrentRequest();
}
