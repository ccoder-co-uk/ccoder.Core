// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Dependencies.Api;
using System.IO;

namespace Web.Brokers.Api;

internal sealed class ApiScriptExecutionBroker(
    ApiScriptExecutionDependency apiScriptExecutionDependency)
    : IApiScriptExecutionBroker
{
    public ValueTask<string> ExecuteScriptAsync(string script) =>
        apiScriptExecutionDependency.ExecuteScriptAsync(script: script);

    public async ValueTask<string> ReadRequestBodyAsync(Stream requestBody)
    {
        using StreamReader reader = new(stream: requestBody);

        return await reader.ReadToEndAsync();
    }
}