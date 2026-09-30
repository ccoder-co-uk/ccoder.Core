// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Web.Models;
using System.IO;

namespace Web.Services.Orchestrations.Api;

public interface IApiScriptOrchestrationService
{
    ValueTask<string> ExecuteApiScriptRequestAsync(
        ApiScriptRequest apiScriptRequest);

    ValueTask<string> ReadRequestBodyAsync(Stream requestBody);
    void LogError(Exception exception);
}