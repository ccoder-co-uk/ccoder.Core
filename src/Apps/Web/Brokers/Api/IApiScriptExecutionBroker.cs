// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Brokers.Api;

internal interface IApiScriptExecutionBroker
{
    ValueTask<string> ExecuteScriptAsync(
        string script);
}