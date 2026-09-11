// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Azure.Functions.Worker.Http;
using Workflow.Dependencies;

namespace Workflow.Brokers.WorkflowFunctions;

internal sealed class WorkflowFunctionHttpBroker(
    WorkflowFunctionHttpDependency workflowFunctionHttpDependency)
        : IWorkflowFunctionHttpBroker
{
    public ValueTask<string> ReadBodyAsync(HttpRequestData request) =>
        workflowFunctionHttpDependency.ReadBodyAsync(request: request);

    public Task<HttpResponseData> CreateResponseAsync(
        HttpRequestData request,
        string content) =>
        workflowFunctionHttpDependency.CreateResponseAsync(
            request: request,
            content: content);
}