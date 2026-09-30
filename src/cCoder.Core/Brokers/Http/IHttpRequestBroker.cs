// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http;
namespace cCoder.Core.Brokers.Http;

internal interface IHttpRequestBroker
{
    HttpRequest GetCurrentRequest();
}