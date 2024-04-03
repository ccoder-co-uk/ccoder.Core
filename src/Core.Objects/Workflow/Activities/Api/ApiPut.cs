using Core.Objects.Dtos.Workflow;
using Core.Objects.Extensions;
using System.Text;

namespace Core.Objects.Workflow.Activities.Api;

public class ApiPut<T, TResult> : ApiActivity<TResult>
{
    [ApiIgnore]
    [IgnoreWhenFlowComplete]
    public T Data { get; set; }

    public bool AutoWrapForOdata { get; set; } = true;

    public bool WaitForResults { get; set; } = true;

    public override async Task Execute()
    {
        using HttpClient api = GetHttpClient();
        Log(WorkflowLogLevel.Info, $"HTTP PUT {BaseUrl}{Query}");

        object payload = AutoWrapForOdata && typeof(T).GetInterface("IEnumerable") != null
            ? new { value = Data }
            : Data;

        if (WaitForResults)
        {
            // wait for the results to come back
            string body = (Data is string d)
                ? d
                : payload.ToJsonForOdata();

            HttpResponseMessage response = await api.PutAsync(Query, new StringContent(body, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                Log(WorkflowLogLevel.Error, $"HTTP PUT {BaseUrl}{Query} failed with status code {(int)response.StatusCode}\n");
                string content = await response.Content.ReadAsStringAsync();
                Log(WorkflowLogLevel.Error, content);
                return;
            }

            if (typeof(TResult) == typeof(string))
                Result = (TResult)(object)await response.Content.ReadAsStringAsync();
            else
            {
                try
                {
                    Result = await response.Content.ReadAsAsync<TResult>();
                }
                catch (Exception ex)
                {
                    Log(WorkflowLogLevel.Error, $"Exception {ex.Message}");
                    Log(WorkflowLogLevel.Error, await response.Content.ReadAsStringAsync());
                }
            }
        }
        else // fire and forget
        {
            Task.Run(async () =>
            {
                using HttpClient api = GetHttpClient();
                api.Timeout = TimeSpan.FromMinutes(10);
                _ = await api.PutAsync(Query, new StringContent(payload.ToJsonForOdata(), Encoding.UTF8, "application/json"));
            }).Forget();
        }
    }
}