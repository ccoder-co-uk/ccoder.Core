using cCoder.Core.Objects.Attributes;
using cCoder.Core.Objects.Dtos.Workflow;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Extensions;
using cCoder.Core.Objects.Workflow.Activities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Dynamic;
using System.Net;
using System.Security;

namespace cCoder.Core.Objects.Entities.Workflow;

[Table("WorkFlows", Schema = "Workflow")]
[Parent("Process")]
public class FlowDefinition : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [ForeignKey("App")]
    public int AppId { get; set; }

    public string DefinitionJson { get; set; }

    public string ConfigJson { get; set; }

    public string ReportingComponentName { get; set; }

    public string InstanceReportingComponentName { get; set; }

    [DontPrivilege]
    public Flow GetFlow() => DefinitionJson != null ? Data.ParseJson<Flow>(DefinitionJson) : null;

    [DontPrivilege]
    public dynamic GetConfig() => ConfigJson != null ? Data.ParseJson<ExpandoObject>(ConfigJson) : null;

    public virtual App App { get; set; }

    public virtual ICollection<FlowInstanceData> Instances { get; set; }

    [DontPrivilege]
    public async Task<Guid> QueueNewInstance(ICoreDataContext core, User asUser, string args)
    {
        if (!asUser.IsAdminOfApp(AppId) && !asUser.Can(AppId, "flowdefinition_execute"))
            throw new SecurityException("Access Denied!");

        Guid instanceId = Guid.NewGuid();

        WorkflowContext context = new()
        {
            ExecutionState = "Queued",
            InstanceId = instanceId,
            Flow = GetFlow(),
            Variables = new Dictionary<string, object>()
            {
                { "Data", args }
            },
            ExecutionLog = Array.Empty<WorkflowLogEntry>()
        };

        ((Start)context.Flow.Activities.First(f => f is Start)).Data = Data.ParseJson(args);

        FlowInstanceData instance = new()
        {
            Id = instanceId,
            State = "Queued",
            FlowDefinitionId = Id,
            Start = DateTimeOffset.UtcNow,
            Caller = asUser.Id,
            ContextString = context.ToJson()
        };

        return (await core.AddAsync(instance)).Id;
    }

    public async ValueTask Execute(Config config, Guid instanceId, string token) =>
        await SendToWorkflow(config.Services["Workflow"], BuildRequest(config, token, instanceId));

    private WorkflowRequest BuildRequest(Config config, string token, Guid instanceId)
    {
        string sslPort = config.Settings["sslPort"] ?? "443";
        string apiUrl = $"https://{App.Domain}:{sslPort}/Api/";

        return new WorkflowRequest
        {
            Api = apiUrl,
            FlowId = Id,
            AuthToken = token,
            InstanceId = instanceId
        };
    }

    private async ValueTask SendToWorkflow(string flowService, WorkflowRequest request)
    {
        if (flowService.StartsWith("http"))
        {
            using HttpClient api = new(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
            {
                BaseAddress = new Uri(flowService)
            };

            api.Timeout = TimeSpan.FromMinutes(11);

            HttpResponseMessage response = await api.PostAsync("Execute", new StringContent(request.ToJson(), System.Text.Encoding.UTF8, "application/json"));
            _ = response.EnsureSuccessStatusCode();
        }
        else
        {
            string requestJson = request.ToJson().Replace("\"", "\\\"");
            _ = System.Diagnostics.Process.Start(new ProcessStartInfo(flowService) { Arguments = $"\"{requestJson}\"", UseShellExecute = true, CreateNoWindow = true });
        }
    }
}
