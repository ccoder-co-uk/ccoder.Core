// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Azure.Functions.Worker.Http;

namespace Workflow.Services.Processings.WorkflowFunctions;

public interface IWorkflowFunctionsProcessingService
{
    Task<HttpResponseData> ProcessExecuteAsync(HttpRequestData request);

    Task<HttpResponseData> ProcessExecuteScriptAsync(
        HttpRequestData request,
        bool useDetails);

    Task<HttpResponseData> ProcessHealthAsync(HttpRequestData request);

    Task ProcessServiceBusMessageAsync(string message);
}