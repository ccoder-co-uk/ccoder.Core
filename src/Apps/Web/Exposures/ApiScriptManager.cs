// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Models;
using System.IO;
using Web.Services.Orchestrations.Api;

namespace Web.Exposures;

internal sealed class ApiScriptManager(
    IApiScriptOrchestrationService apiScriptOrchestrationService)
    : IApiScriptManager
{
    public ValueTask<string> ExecuteApiScriptRequestAsync(
        ApiScriptRequest apiScriptRequest) =>
        apiScriptOrchestrationService.ExecuteApiScriptRequestAsync(
            apiScriptRequest: apiScriptRequest);

    public ValueTask<string> ReadRequestBodyAsync(Stream requestBody) =>
        apiScriptOrchestrationService.ReadRequestBodyAsync(requestBody: requestBody);

    public void LogError(Exception exception) =>
        apiScriptOrchestrationService.LogError(exception: exception);
}