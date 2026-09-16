// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Brokers.Api;
using System.IO;

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

    public ValueTask<string> ReadRequestBodyAsync(Stream requestBody) =>
        TryCatch(operation: async () =>
        {
            ValidateRequestBodyOnRead(requestBody: requestBody);

            return await apiScriptExecutionBroker.ReadRequestBodyAsync(
                requestBody: requestBody);
        });
}