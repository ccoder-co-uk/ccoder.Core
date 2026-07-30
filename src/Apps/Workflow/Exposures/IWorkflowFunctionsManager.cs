// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Azure.Functions.Worker.Http;

namespace Workflow.Exposures;

public interface IWorkflowFunctionsManager
{
    Task<HttpResponseData> ProcessExecuteAsync(HttpRequestData request);

    Task<HttpResponseData> ProcessExecuteScriptAsync(
        HttpRequestData request,
        bool useDetails);

    Task<HttpResponseData> ProcessHealthAsync(HttpRequestData request);

    Task ProcessServiceBusMessageAsync(string message);
}
