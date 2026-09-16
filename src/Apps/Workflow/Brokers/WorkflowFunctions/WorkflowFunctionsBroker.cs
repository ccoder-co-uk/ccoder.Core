// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Activities.Models;
using Microsoft.Azure.Functions.Worker.Http;
using Workflow.Dependencies;

namespace Workflow.Brokers.WorkflowFunctions;

internal sealed class WorkflowFunctionsBroker(
    WorkflowFunctionsDependency workflowFunctionsDependency)
    : IWorkflowFunctionsBroker
{
    public ValueTask<string> ReadBodyAsync(HttpRequestData request) =>
        workflowFunctionsDependency.ReadBodyAsync(request: request);

    public WorkflowRequest DeserializeWorkflowRequest(string json) =>
        workflowFunctionsDependency.DeserializeWorkflowRequest(json: json);

    public Task RunWorkflowRequestAsync(WorkflowRequest workflowRequest) =>
        workflowFunctionsDependency.RunWorkflowRequestAsync(
            workflowRequest: workflowRequest);

    public Task<string> ExecuteScriptAsync(string payload, bool useDetails) =>
        workflowFunctionsDependency.ExecuteScriptAsync(
            payload: payload,
            useDetails: useDetails);

    public Task<HttpResponseData> CreateResponseAsync(
        HttpRequestData request,
        string content) =>
        workflowFunctionsDependency.CreateResponseAsync(
            request: request,
            content: content);

    public void LogInformation(string message) =>
        workflowFunctionsDependency.LogInformation(message: message);
}