// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Workflow.Exposures;

namespace Workflow.Exposures;

public sealed class Execute(
    IWorkflowFunctionsManager workflowFunctionsProcessingService)
{
    [Function(nameof(Execute))]
    public Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request) =>
        workflowFunctionsProcessingService.ProcessExecuteAsync(request: request);
}