// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Activities.Models;
using Microsoft.Azure.Functions.Worker.Http;
using Workflow.Services.Foundations.WorkflowFunctions;

namespace Workflow.Services.Processings.WorkflowFunctions;

internal sealed partial class WorkflowFunctionsProcessingService(
    IWorkflowFunctionsService workflowFunctionsService)
        : IWorkflowFunctionsProcessingService
{
    public Task<HttpResponseData> ProcessExecuteAsync(HttpRequestData request) =>
        TryCatch(operation: async () =>
        {
            ValidateInputs(inputs: [request]);

            string json = await workflowFunctionsService.ReadBodyAsync(
                request: request);

            WorkflowRequest workflowRequest =
                workflowFunctionsService.DeserializeWorkflowRequest(
                    json: json)
                ?? throw new InvalidOperationException(
                    message:
                        "Workflow request payload could not be deserialized.");

            await workflowFunctionsService.RunWorkflowRequestAsync(
                workflowRequest: workflowRequest);

            return await workflowFunctionsService.CreateHttpResponseDataAsync(
                request: request,
                content: "OK");
        });

    public Task<HttpResponseData> ProcessExecuteScriptAsync(
        HttpRequestData request,
        bool useDetails) =>
        TryCatch(operation: async () =>
        {
            ValidateInputs(inputs: [request, useDetails]);

            string payload = await workflowFunctionsService.ReadBodyAsync(
                request: request);

            string result = await workflowFunctionsService.ExecuteScriptAsync(
                payload: payload,
                useDetails: useDetails);

            return await workflowFunctionsService.CreateHttpResponseDataAsync(
                request: request,
                content: result);
        });

    public Task<HttpResponseData> ProcessHealthAsync(HttpRequestData request) =>
        TryCatch(operation: () =>
        {
            ValidateInputs(inputs: [request]);

            return workflowFunctionsService.CreateHttpResponseDataAsync(
                request: request,
                content: "OK");
        });

    public Task ProcessServiceBusMessageAsync(string message) =>
        TryCatch(operation: () =>
        {
            ValidateInputs(inputs: [message]);

            workflowFunctionsService.LogInformation(
                message:
                    "Service Bus workflow trigger is scaffolded but disabled.");

            return Task.CompletedTask;
        });

}