// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Activities.Models;
using Microsoft.Azure.Functions.Worker.Http;

namespace Workflow.Brokers.WorkflowFunctions;

internal interface IWorkflowFunctionsBroker
{
    ValueTask<string> ReadBodyAsync(HttpRequestData request);
    WorkflowRequest DeserializeWorkflowRequest(string json);
    Task RunWorkflowRequestAsync(WorkflowRequest workflowRequest);
    Task<string> ExecuteScriptAsync(string payload, bool useDetails);
    Task<HttpResponseData> CreateResponseAsync(
        HttpRequestData request,
        string content);
    void LogInformation(string message);
}