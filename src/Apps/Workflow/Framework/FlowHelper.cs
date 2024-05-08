using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Objects.Extensions;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Workflow.Framework;

internal static class FlowHelper
{
    public static async Task<FlowInstance> GetInstance(Guid instanceId, string apiRoot, string auth, LogEvent log)
    {
        using HttpClient api = Api(apiRoot);
        if (auth != null)
        {
            _ = api.WithAuthToken(auth);
        }

        FlowInstanceData def = JsonConvert.DeserializeObject<FlowInstanceData>(await api.GetStringAsync($"Core/FlowInstanceData({instanceId})?$expand=FlowDefinition($select=Id,AppId)"), ObjectExtensions.GetJSONSettings());
        return new FlowInstance(def, log);
    }

    public static async Task SaveResult(FlowInstanceData result, string apiRoot, string auth)
    {
        using HttpClient api = Api(apiRoot);
        if (auth != null)
        {
            _ = api.WithAuthToken(auth);
        }

        FlowInstanceData payload = new()
        {
            Id = result.Id,
            FlowDefinitionId = result.FlowDefinitionId,
            Name = result.Name,
            ContextString = result.ContextString,
            State = result.State,
            ReportingComponentName = result.ReportingComponentName,
            Caller = result.Caller,
            Start = result.Start,
            End = result.End
        };
        HttpResponseMessage response = await api.PutAsync($"Core/FlowInstanceData({result.Id})", new StringContent(payload.ToJsonForOdata(), Encoding.UTF8, "application/json"));
        _ = response.EnsureSuccessStatusCode();
    }

    private static HttpClient Api(string apiBase)
        => new(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }) { BaseAddress = new Uri(apiBase) };

}