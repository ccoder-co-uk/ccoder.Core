// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Workflow.Exposures;

namespace Workflow.Exposures;

public sealed class ExecuteScript(
    IWorkflowFunctionsManager workflowFunctionsProcessingService)
{
    [Function(nameof(ExecuteScript))]
    public Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request,
        [FromQuery] bool useDetails = false) =>
        workflowFunctionsProcessingService.ProcessExecuteScriptAsync(
            request: request,
            useDetails: useDetails);
}