// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;
using System.IO;

namespace Web.Brokers.Api;

internal interface IApiContextBroker
{
    ApiInfo[] SelectAllApiInfos();
    ValueTask<string> ReadRequestBodyAsync(Stream requestBody);
    void LogError(Exception exception);
}