// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Brokers.Http;

internal interface IHttpRequestBroker
{
    HttpRequest GetCurrentRequest();
}