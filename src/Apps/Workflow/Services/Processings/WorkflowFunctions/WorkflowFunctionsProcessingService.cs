// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Activities.Models;
using cCoder.Workflow.Engine.Exposures;
using cCoder.Workflow.Engine.Support;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace Workflow.Services.Processings.WorkflowFunctions;

internal sealed partial class WorkflowFunctionsProcessingService(
    IFlowRunner flowRunner,
    IWorkflowScriptExecutionService scriptExecutionService,
    ILogger<WorkflowFunctionsProcessingService> logger)
        : IWorkflowFunctionsProcessingService
{
    public Task<HttpResponseData> ProcessExecuteAsync(HttpRequestData request) =>
        TryCatch(operation: async () =>
        {
            ValidateInputs(inputs: [request]);

            string json = await new StreamReader(request.Body)
                .ReadToEndAsync();

            WorkflowRequest workflowRequest =
                JsonConvert.DeserializeObject<WorkflowRequest>(
                    value: json,
                    settings: WorkflowJson.GetJsonSettings())
                ?? throw new InvalidOperationException(
                    message:
                        "Workflow request payload could not be deserialized.");

            await flowRunner.RunAsync(request: workflowRequest);

            return await CreateHttpResponseDataAsync(
                request: request,
                content: "OK");
        });

    public Task<HttpResponseData> ProcessExecuteScriptAsync(
        HttpRequestData request,
        bool useDetails) =>
        TryCatch(operation: async () =>
        {
            ValidateInputs(inputs: [request, useDetails]);

            string payload = await new StreamReader(request.Body)
                .ReadToEndAsync();

            string result = await scriptExecutionService.ExecuteAsync(
                payload: payload,
                useDetails: useDetails);

            return await CreateHttpResponseDataAsync(
                request: request,
                content: result);
        });

    public Task<HttpResponseData> ProcessHealthAsync(HttpRequestData request) =>
        TryCatch(operation: () =>
        {
            ValidateInputs(inputs: [request]);

            return CreateHttpResponseDataAsync(
                request: request,
                content: "OK");
        });

    public Task ProcessServiceBusMessageAsync(string message) =>
        TryCatch(operation: () =>
        {
            ValidateInputs(inputs: [message]);

            logger.LogInformation(
                message:
                    "Service Bus workflow trigger is scaffolded but disabled.");

            return Task.CompletedTask;
        });

    private static async Task<HttpResponseData> CreateHttpResponseDataAsync(
        HttpRequestData request,
        string content)
    {
        HttpResponseData response = request.CreateResponse(
            statusCode: HttpStatusCode.OK);

        await response.WriteStringAsync(value: content);

        return response;
    }
}