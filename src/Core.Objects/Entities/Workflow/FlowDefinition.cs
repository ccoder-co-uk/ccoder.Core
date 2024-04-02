using Core.Objects.Dtos.Workflow;
using Core.Objects.Entities.CMS;
using Core.Objects.Entities.Security;
using Core.Objects.Extensions;
using Core.Objects.Workflow.Activities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;

namespace Core.Objects.Entities.Workflow
{
    [Table("WorkFlows", Schema = "Workflow")]
    [Parent("Process")]
    public class FlowDefinition : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [ForeignKey("App")]
        public int AppId { get; set; }

        [ForeignKey("Process")]
        public Guid? ProcessId { get; set; }

        public string DefinitionJson { get; set; }

        public string ConfigJson { get; set; }

        public string ReportingComponentName { get; set; }

        public string InstanceReportingComponentName { get; set; }

        [DontPrivilege]
        public Flow GetFlow() => DefinitionJson != null ? Data.ParseJson<Flow>(DefinitionJson) : null;

        [DontPrivilege]
        public dynamic GetConfig() => ConfigJson != null ? Data.ParseJson<ExpandoObject>(ConfigJson) : null;

        public virtual App App { get; set; }
        public virtual BusinessProcess Process { get; set; }

        public virtual ICollection<FlowInstanceData> Instances { get; set; }

        [DontPrivilege]
        public async Task<Guid> QueueNewInstance(ICoreDataContext core, User asUser, string args)
        {
            if (!asUser.IsAdminOfApp(AppId) && !asUser.Can(AppId, "flowdefinition_execute"))
                throw new SecurityException("Access Denied!");

            var instanceId = Guid.NewGuid();

            var context = new WorkflowContext
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

            var instance = new FlowInstanceData
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

        WorkflowRequest BuildRequest(Config config, string token, Guid instanceId)
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

        async ValueTask SendToWorkflow(string flowService, WorkflowRequest request)
        {
            if (flowService.StartsWith("http"))
            {
                using HttpClient api = new(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
                {
                    BaseAddress = new Uri(flowService)
                };

                api.Timeout = TimeSpan.FromMinutes(11);

                var response = await api.PostAsync("Execute", new StringContent(request.ToJson(), System.Text.Encoding.UTF8, "application/json"));
                _ = response.EnsureSuccessStatusCode();
            }
            else
            {
                string requestJson = request.ToJson().Replace("\"", "\\\"");
                _ = System.Diagnostics.Process.Start(new ProcessStartInfo(flowService) { Arguments = $"\"{requestJson}\"", UseShellExecute = true, CreateNoWindow = true });
            }
        }
    }
}
