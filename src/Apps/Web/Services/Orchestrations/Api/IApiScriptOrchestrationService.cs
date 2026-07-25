// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Models;

namespace Web.Services.Orchestrations.Api;

internal interface IApiScriptOrchestrationService
{
    ValueTask<string> ExecuteApiScriptRequestAsync(
        ApiScriptRequest request);
}