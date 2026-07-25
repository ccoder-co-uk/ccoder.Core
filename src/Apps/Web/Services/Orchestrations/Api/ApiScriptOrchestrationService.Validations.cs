// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Models;

namespace Web.Services.Orchestrations.Api;

internal sealed partial class ApiScriptOrchestrationService
{
    private static void ValidateApiScriptRequestOnExecute(
        ApiScriptRequest request) =>
        ArgumentNullException.ThrowIfNull(
            argument: request);
}