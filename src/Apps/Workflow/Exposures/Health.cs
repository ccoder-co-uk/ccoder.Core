// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Workflow.Services.Processings.WorkflowFunctions;

namespace Workflow.Exposures;

public sealed class Health(
    IWorkflowFunctionsProcessingService workflowFunctionsProcessingService)
{
    [Function(nameof(Health))]
    public Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Health")]
        HttpRequestData request) =>
        workflowFunctionsProcessingService.ProcessHealthAsync(request: request);
}