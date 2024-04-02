using Core.Objects.Entities.CMS;
using Core.Objects.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Objects.Workflow.Activities
{
    public sealed class Start : CoreActivity
    {
        public dynamic Data { get; set; }
        private IWorkflowContext Context { get; set; }
        public override async Task Execute()
        {
            if (Data != null)
            {
                Log(Dtos.Workflow.WorkflowLogLevel.Debug, JsonConvert.SerializeObject(Data, ObjectExtensions.GetODataJsonSettings()));
            }

            if (Context.Variables.ContainsKey("AppId"))
            {
                using System.Net.Http.HttpClient api = GetHttpClient();
                App app = await api.GetAsync<App>($"Core/App({Context.Variables["AppId"]})");
                Context.Variables.Add(new KeyValuePair<string, object>("App", app));
                Log(Dtos.Workflow.WorkflowLogLevel.Info, "Grabbed app information");
            }
            await base.Execute();
        }

        public override async Task ExecuteInternal(IWorkflowContext context)
        {
            Context = context;
            await base.ExecuteInternal(context);
        }
    }
}