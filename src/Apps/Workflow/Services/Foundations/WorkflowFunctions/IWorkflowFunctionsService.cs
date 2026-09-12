// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Activities.Models;
using Microsoft.Azure.Functions.Worker.Http;

namespace Workflow.Services.Foundations.WorkflowFunctions;

internal interface IWorkflowFunctionsService
{
    ValueTask<string> ReadBodyAsync(HttpRequestData request);
    WorkflowRequest DeserializeWorkflowRequest(string json);
    Task RunWorkflowRequestAsync(WorkflowRequest workflowRequest);
    Task<string> ExecuteScriptAsync(string payload, bool useDetails);
    Task<HttpResponseData> CreateHttpResponseDataAsync(
        HttpRequestData request,
        string content);
    void LogInformation(string message);
}