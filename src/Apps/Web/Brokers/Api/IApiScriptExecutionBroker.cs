// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.IO;

namespace Web.Brokers.Api;

internal interface IApiScriptExecutionBroker
{
    ValueTask<string> ExecuteScriptAsync(
        string script);

    ValueTask<string> ReadRequestBodyAsync(Stream requestBody);
}