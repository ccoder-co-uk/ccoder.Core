// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Activities.Models;
using Microsoft.Azure.Functions.Worker.Http;
using Workflow.Brokers.WorkflowFunctions;

namespace Workflow.Services.Foundations.WorkflowFunctions;

internal sealed partial class WorkflowFunctionsService(
    IWorkflowFunctionsBroker workflowFunctionsBroker)
        : IWorkflowFunctionsService
{
    public ValueTask<string> ReadBodyAsync(HttpRequestData request) =>
        TryCatch(operation: async ValueTask<string> () =>
        {
            ValidateInputs(inputs: [request]);

            return await workflowFunctionsBroker.ReadBodyAsync(
                request: request);
        });

    public WorkflowRequest DeserializeWorkflowRequest(string json) =>
        TryCatch(operation: () =>
        {
            ValidateInputs(inputs: [json]);

            return workflowFunctionsBroker.DeserializeWorkflowRequest(json: json);
        });

    public Task RunWorkflowRequestAsync(WorkflowRequest workflowRequest) =>
        TryCatch(operation: async () =>
        {
            ValidateInputs(inputs: [workflowRequest]);

            await workflowFunctionsBroker.RunWorkflowRequestAsync(
                workflowRequest: workflowRequest);
        });

    public Task<string> ExecuteScriptAsync(string payload, bool useDetails) =>
        TryCatch(operation: async Task<string> () =>
        {
            ValidateInputs(inputs: [payload, useDetails]);

            return await workflowFunctionsBroker.ExecuteScriptAsync(
                payload: payload,
                useDetails: useDetails);
        });

    public Task<HttpResponseData> CreateHttpResponseDataAsync(
        HttpRequestData request,
        string content) =>
        TryCatch(operation: async Task<HttpResponseData> () =>
        {
            ValidateInputs(inputs: [request, content]);

            return await workflowFunctionsBroker.CreateResponseAsync(
                request: request,
                content: content);
        });

    public void LogInformation(string message) =>
        TryCatch(operation: () =>
        {
            ValidateInputs(inputs: [message]);

            workflowFunctionsBroker.LogInformation(message: message);
        });
}