// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Models;
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
}