// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using cCoder.Workflow.Activities.Models;
using cCoder.Workflow.Engine.Exposures;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Workflow.Dependencies;

internal sealed class WorkflowFunctionsDependency : IWorkflowScriptExecutionService
{
    private readonly IFlowRunner workflowRunner;
    private readonly IWorkflowScriptExecutionService scriptExecutionService;
    private readonly ILogger<WorkflowFunctionsDependency> logger;
    private readonly Encoding encoding = Encoding.UTF8;
    private readonly JsonSerializerOptions jsonSerializerOptions =
        new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

    public WorkflowFunctionsDependency(
        IFlowRunner flowRunner,
        IWorkflowScriptExecutionService workflowScriptExecutionService,
        ILogger<WorkflowFunctionsDependency> logger)
    {
        this.workflowRunner = flowRunner;
        this.scriptExecutionService = workflowScriptExecutionService;
        this.logger = logger;
    }

    public async ValueTask<string> ReadBodyAsync(HttpRequestData request)
    {
        using MemoryStream content = new();
        await request.Body.CopyToAsync(destination: content);

        return encoding.GetString(bytes: content.ToArray());
    }

    public WorkflowRequest DeserializeWorkflowRequest(string json) =>
        JsonSerializer.Deserialize<WorkflowRequest>(
            json: json,
            options: jsonSerializerOptions);

    public Task RunWorkflowRequestAsync(WorkflowRequest workflowRequest) =>
        workflowRunner.RunAsync(workflowRequest: workflowRequest);

    public Task<string> ExecuteScriptAsync(string payload, bool useDetails) =>
        scriptExecutionService.ExecuteAsync(
            payload: payload,
            useDetails: useDetails);

    public Task<string> ExecuteAsync(string payload, bool useDetails) =>
        ExecuteScriptAsync(payload: payload, useDetails: useDetails);

    public async Task<HttpResponseData> CreateResponseAsync(
        HttpRequestData request,
        string content)
    {
        HttpResponseData response = request.CreateResponse(
            statusCode: HttpStatusCode.OK);

        await response.WriteStringAsync(value: content);

        return response;
    }

    public void LogInformation(string message) =>
        logger.LogInformation(message: message);

}