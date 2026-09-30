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
using Microsoft.Azure.Functions.Worker.Http;

namespace Workflow.Services.Processings.WorkflowFunctions;

internal interface IWorkflowFunctionsProcessingService
{
    Task<HttpResponseData> ProcessExecuteAsync(HttpRequestData request);

    Task<HttpResponseData> ProcessExecuteScriptAsync(
        HttpRequestData request,
        bool useDetails);

    Task<HttpResponseData> ProcessHealthAsync(HttpRequestData request);
}