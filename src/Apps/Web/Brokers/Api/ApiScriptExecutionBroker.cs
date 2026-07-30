// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Dependencies.Api;

namespace Web.Brokers.Api;

internal sealed class ApiScriptExecutionBroker(
    ApiScriptExecutionDependency apiScriptExecutionDependency)
    : IApiScriptExecutionBroker
{
    public ValueTask<string> ExecuteScriptAsync(string script) =>
        apiScriptExecutionDependency.ExecuteScriptAsync(script: script);
}