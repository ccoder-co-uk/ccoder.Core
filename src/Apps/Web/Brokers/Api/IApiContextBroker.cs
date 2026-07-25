// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;

namespace Web.Brokers.Api;

internal interface IApiContextBroker
{
    ApiInfo[] SelectAllApiInfos();
}