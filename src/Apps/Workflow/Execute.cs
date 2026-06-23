using cCoder.Workflow.Activities.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using Workflow.Services;

namespace Workflow;

public sealed class Execute(WorkflowExecutionService executionService)
{
    [Function(nameof(Execute))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        string json = await new StreamReader(request.Body).ReadToEndAsync();
        WorkflowRequest workflowRequest = JsonConvert.DeserializeObject<WorkflowRequest>(json, WorkflowJson.GetJsonSettings())
            ?? throw new InvalidOperationException("Workflow request payload could not be deserialized.");

        await executionService.ExecuteAsync(workflowRequest);

        HttpResponseData response = request.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteStringAsync("OK");
        return response;
    }
}
