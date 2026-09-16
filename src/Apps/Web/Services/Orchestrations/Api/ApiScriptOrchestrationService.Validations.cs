// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Models;
using System.IO;

namespace Web.Services.Orchestrations.Api;

internal sealed partial class ApiScriptOrchestrationService
{
    private static void ValidateApiScriptRequestOnExecute(
        ApiScriptRequest apiScriptRequest) =>
        ArgumentNullException.ThrowIfNull(
            argument: apiScriptRequest);

    private static void ValidateRequestBodyOnRead(Stream requestBody) =>
        ArgumentNullException.ThrowIfNull(argument: requestBody);
}