using Core.Objects.Dtos.Workflow;
using Core.Objects.Extensions;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Core.Objects.Workflow.Activities.Api;

public class ApiPostBatch : ApiActivity<BatchedResponse[]>
{
    [ApiIgnore]
    public BatchedRequest[] Data { get; set; }

    public override async Task Execute()
    {
        using HttpClient api = GetHttpClient();
        Log(WorkflowLogLevel.Info, $"HTTP POST {BaseUrl}{Query}$batch");
        Log(WorkflowLogLevel.Info, $"Sending a batch of {Data.Length} requests to the API.");

        string body = new { Requests = Data }.ToJsonForOdata();
        HttpResponseMessage response = await api.PostAsync(Query + "$batch", new StringContent(body, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            Log(WorkflowLogLevel.Error, $"HTTP POST {BaseUrl}{Query}$batch failed with status code {(int)response.StatusCode}\n");
            string content = await response.Content.ReadAsStringAsync();
            Log(WorkflowLogLevel.Error, content);
            return;
        }

        try
        {
            var responseBatch = await response.Content.ReadAsAsync<ResponseBatch>();
            Result = responseBatch.Responses;

            Log(WorkflowLogLevel.Info, $"Received {responseBatch.Responses.Length} batched responses");
            Log(WorkflowLogLevel.Info, $"Received {responseBatch.Responses.Where(r => r.Status.StartsWith("2")).Count()} successes");

            var failures = responseBatch.Responses.Where(r => !r.Status.StartsWith("2")).ToArray();

            if (failures.Any())
            {
                Log(WorkflowLogLevel.Warning, $"Received {failures.Length} failures");

                foreach (var failure in failures)
                    Log(WorkflowLogLevel.Error, failure.Body.ToJsonForOdata());
            }
        }
        catch (Exception ex)
        {
            Log(WorkflowLogLevel.Error, $"Exception {ex.Message}");
            Log(WorkflowLogLevel.Error, await response.Content.ReadAsStringAsync());
        }
    }
}

public class BatchedRequest
{
    public string Id { get; set; }
    public string Method { get; set; }
    public string Url { get; set; }
}

public class ResponseBatch
{
    public BatchedResponse[] Responses { get; set; }
}

public class BatchedResponse
{
    public string Id { get; set; }
    public string Status { get; set; }
    public object Headers { get; set; }
    public object Body { get; set; }
}