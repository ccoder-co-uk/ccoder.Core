// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Workflow.Services.Processings.WorkflowFunctions;

namespace Workflow.Exposures;

internal sealed class Execute(
    IWorkflowFunctionsProcessingService workflowFunctionsProcessingService)
{
    [Function(nameof(Execute))]
    public Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request) =>
        workflowFunctionsProcessingService.ProcessExecuteAsync(request: request);
}