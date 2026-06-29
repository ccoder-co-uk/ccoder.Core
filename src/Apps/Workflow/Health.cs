using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Workflow;

public sealed class Health
{
    [Function(nameof(Health))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Health")] HttpRequestData request)
    {
        HttpResponseData response = request.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteStringAsync("OK");
        return response;
    }
}
