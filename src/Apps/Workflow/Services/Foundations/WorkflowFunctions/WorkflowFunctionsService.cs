// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Activities.Models;
using Microsoft.Azure.Functions.Worker.Http;
using Workflow.Brokers.Loggings;
using Workflow.Brokers.WorkflowFunctions;

namespace Workflow.Services.Foundations.WorkflowFunctions;

internal sealed partial class WorkflowFunctionsService(
    IWorkflowFunctionHttpBroker workflowFunctionHttpBroker,
    IWorkflowJsonBroker workflowJsonBroker,
    IWorkflowRunnerBroker workflowRunnerBroker,
    IWorkflowScriptExecutionBroker workflowScriptExecutionBroker,
    ILoggingBroker loggingBroker)
        : IWorkflowFunctionsService
{
    public ValueTask<string> ReadBodyAsync(HttpRequestData request) =>
        TryCatch(operation: async ValueTask<string> () =>
        {
            ValidateInputs(inputs: [request]);

            return await workflowFunctionHttpBroker.ReadBodyAsync(
                request: request);
        });

    public WorkflowRequest DeserializeWorkflowRequest(string json) =>
        TryCatch(operation: () =>
        {
            ValidateInputs(inputs: [json]);

            return workflowJsonBroker.DeserializeWorkflowRequest(json: json);
        });

    public Task RunWorkflowRequestAsync(WorkflowRequest workflowRequest) =>
        TryCatch(operation: async () =>
        {
            ValidateInputs(inputs: [workflowRequest]);

            await workflowRunnerBroker.RunWorkflowRequestAsync(
                workflowRequest: workflowRequest);
        });

    public Task<string> ExecuteScriptAsync(string payload, bool useDetails) =>
        TryCatch(operation: async Task<string> () =>
        {
            ValidateInputs(inputs: [payload, useDetails]);

            return await workflowScriptExecutionBroker.ExecuteAsync(
                payload: payload,
                useDetails: useDetails);
        });

    public Task<HttpResponseData> CreateHttpResponseDataAsync(
        HttpRequestData request,
        string content) =>
        TryCatch(operation: async Task<HttpResponseData> () =>
        {
            ValidateInputs(inputs: [request, content]);

            return await workflowFunctionHttpBroker.CreateResponseAsync(
                request: request,
                content: content);
        });

    public void LogInformation(string message) =>
        TryCatch(operation: () =>
        {
            ValidateInputs(inputs: [message]);

            loggingBroker.LogInformation(message: message);
        });
}