using Core.Objects.Dtos.Workflow;
using Core.Objects.Extensions;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Objects.Workflow.Activities
{
    /// <summary>
    /// Base type for all workflow activities
    /// </summary>
    public abstract class Activity
    {
        public static IDictionary<Activity, Action<Activity, IDictionary<string, object>, Flow>> CompiledLinkCache { get; set; }

        public static readonly string[] ScriptImports =
        [
            "B2B.Objects",
            "B2B.Objects.Dtos",
            "B2B.Objects.Entities",
            "B2B.Objects.Entities.Masterdata",
            "B2B.Objects.Entities.Transactions",
            "B2B.Objects.Entities.Funding",
            "B2B.Objects.Entities.Payments",
            "B2B.Objects.Workflow.Activities",
            "Core.Connectivity.Workflow.Sftp",
            "Core.Objects",
            "Core.Objects.Dtos",
            "Core.Objects.Dtos.Workflow",
            "Core.Objects.Extensions",
            "Core.Objects.Entities",
            "Core.Objects.Entities.CMS",
            "Core.Objects.Entities.DMS",
            "Core.Objects.Entities.Security",
            "Core.Objects.Entities.Planning",
            "Core.Objects.Workflow.Activities",
            "Core.Objects.Workflow.Activities.Api",
            "Core.Objects.Workflow.Activities.DMS",
            "Core.Objects.Workflow.Activities.Templating",
            "Core.Objects.Workflow.Activities.Transformation",
            "Newtonsoft.Json",
            "Newtonsoft.Json.Linq",
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "System.Xml.Linq"
        ];

        [Required]
        public string Ref { get; set; }

        public ActivityState State { get; protected set; } = ActivityState.NotRun;

        [NotMapped]
        [JsonIgnore]
        public Activity[] Previous { get; set; }

        [NotMapped]
        [JsonIgnore]
        public Activity[] Next { get; set; }

        [JsonIgnore]
        public string AssignCode { get; set; }

        [NotMapped]
        [JsonIgnore]
        public IScriptRunner ScriptRunner { get; set; }

        public virtual Task Execute() => Task.FromResult(true);

        public void Skip()
        {
            State = ActivityState.Skipped;
            Next.Where(n => n.Previous.Length == 1).ForEach(n => n.Skip());
        }

        public void Log(WorkflowLogLevel level, string message)
        {
            Context?.Log(level, $"{Ref}:: {message}");
            if (level is WorkflowLogLevel.Error or WorkflowLogLevel.Fatal)
            {
                State = ActivityState.Failed;
            }
        }

        private IWorkflowContext Context;

        public virtual async Task ExecuteInternal(IWorkflowContext context)
        {
            Context = context;

            if (Previous == null || Previous.All(a => a.State == ActivityState.Complete))
            {
                Log(WorkflowLogLevel.Info, "Activity Execution started");
                await ExecuteLinksAsync(context);

                if (State == ActivityState.NotRun)
                    await SafeExecuteAndUpdateState(context);
            }
        }

        private async Task SafeExecuteAndUpdateState(IWorkflowContext context)
        {
            try
            {
                State = ActivityState.Running;
                await Execute();

                if (State == ActivityState.Running)
                {
                    State = ActivityState.Complete;
                    Log(WorkflowLogLevel.Info, "Activity Execution completed");
                    await ContinueFlow(context);
                }
                else
                {
                    Log(WorkflowLogLevel.Error, "Activity Execution Failed");
                }
            }
            catch (Exception ex)
            {
                Log(WorkflowLogLevel.Error, "Activity Execution Failed:\n" + ex.Message);
                Log(WorkflowLogLevel.Debug, ex.StackTrace);
                State = ActivityState.Failed;
            }
        }

        private async Task ExecuteLinksAsync(IWorkflowContext context)
        {
            if (!string.IsNullOrEmpty(AssignCode))
            {
                try
                {
                    if (CompiledLinkCache is not null && CompiledLinkCache.ContainsKey(this))
                        CompiledLinkCache[this](this, context.Variables, context.Flow);
                    else
                        (await BuildScript<Action<Activity, IDictionary<string, object>, Flow>>(AssignCode))?.Invoke(this, context.Variables, context.Flow);
                }
                catch (Exception ex)
                {
                    State = ActivityState.Failed;
                    Log(WorkflowLogLevel.Fatal, $"Link Execution Failed\n{ex.Message}\n{ex.StackTrace}");
                    Log(WorkflowLogLevel.Fatal, $"Link Code in question: \n{AssignCode}");
                    return;
                }
            }
        }

        private async Task ContinueFlow(IWorkflowContext context)
        {
            if (Next != null)
                foreach (Activity t in Next)
                    await t.ExecuteInternal(context);
        }

        protected Task<TFunc> BuildScript<TFunc>(string code) => 
            (ScriptRunner ?? Context.Script).BuildScript<TFunc>(code, (string[])Context?.Variables["Imports"] ?? ScriptImports, Log);

        protected Task<T> ExecuteScript<T>(string code, object args) => 
            (ScriptRunner ?? Context.Script).Run<T>(code, (string[])Context?.Variables["Imports"] ?? ScriptImports, args, Log);

        protected Task ExecuteScript(string code, object args) => 
            (ScriptRunner ?? Context.Script).Run(code, (string[])Context?.Variables["Imports"] ?? ScriptImports, args, Log);
    }
}