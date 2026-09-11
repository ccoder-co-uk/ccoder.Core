// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Azure.Functions.Worker.Http;

namespace Workflow.Brokers.WorkflowFunctions;

internal interface IWorkflowFunctionHttpBroker
{
    ValueTask<string> ReadBodyAsync(HttpRequestData request);
    Task<HttpResponseData> CreateResponseAsync(
        HttpRequestData request,
        string content);
}