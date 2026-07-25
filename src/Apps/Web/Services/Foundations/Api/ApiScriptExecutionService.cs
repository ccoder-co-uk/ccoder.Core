// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Brokers.Api;

namespace Web.Services.Foundations.Api;

internal sealed partial class ApiScriptExecutionService(
    IApiScriptExecutionBroker apiScriptExecutionBroker)
    : IApiScriptExecutionService
{
    public ValueTask<string> ExecuteScriptAsync(
        string script) =>
        TryCatch(operation: async () =>
        {
            ValidateScriptOnExecute(script: script);

            return await apiScriptExecutionBroker
                .ExecuteScriptAsync(script: script);
        });
}