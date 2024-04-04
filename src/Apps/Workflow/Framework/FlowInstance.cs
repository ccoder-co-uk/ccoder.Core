using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos.Workflow;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Objects.Extensions;
using cCoder.Core.Objects.Workflow.Activities;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;

namespace Workflow.Framework
{
    public sealed class FlowInstance
    {
        public Guid Id { get; set; }

        public int AppId { get; }

        public string Caller { get; set; }

        public string Name { get; }
        public string FlowDefinition { get; }
        internal Flow Flow { get; private set; }
        public WorkflowContext Context { get; private set; }


        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }

        public event LogEvent OnLog;
        internal IScriptRunner Script { get; private set; }

        private FlowInstance(LogEvent log) => Script = new ScriptRunner(log);

        /// <summary>
        /// Constructs a FlowInstance from raw data from a db
        /// </summary>
        /// <param name="instanceData"></param>
        public FlowInstance(FlowInstanceData instanceData, LogEvent log) : this(log)
        {
            OnLog += log;

            AppId = instanceData.FlowDefinition.AppId;
            Id = instanceData.Id;
            Name = instanceData.Name;
            Start = instanceData.Start;
            FlowDefinition = instanceData.FlowDefinition.ToJson();

            var dtoContext = JsonConvert.DeserializeObject<cCoder.Core.Objects.Dtos.Workflow.WorkflowContext>(Encoding.UTF8.GetString(instanceData.ContextJson), ObjectExtensions.GetJSONSettings());
            Flow = dtoContext.Flow;

            Stitch().Wait();
        }

        public async Task Log(WorkflowLogLevel level, string message)
        {
            try
            {
                await OnLog?.Invoke(level, message);
            }
            catch (Exception ex)
            {
                OnLog = null;
                Context.Log(WorkflowLogLevel.Warning, "Realtime processing has failed:\n" + ex.Message + "\nLog streaming has been disabled.");
            }
        }

        public async Task<FlowInstanceData> Execute(WorkflowRequest request)
        {
            Start = DateTimeOffset.UtcNow;
            Id = request.InstanceId;
            Context = new WorkflowContext(Flow, this);
            await Context.Execute(request.Api, request.AuthToken);
            return Complete();
        }

        private FlowInstanceData Complete()
        {
            FlowDefinition flowDef = JsonConvert.DeserializeObject<FlowDefinition>(FlowDefinition, ObjectExtensions.GetJSONSettings());
            Context.Flow.Activities.ForEach((a) =>
            {
                PropertyInfo[] props = a.GetType().GetProperties().Where(p => p.GetCustomAttribute<IgnoreWhenFlowCompleteAttribute>() != null).ToArray();
                props.ForEach((p) => p.SetValue(a, default));
            });
            return new FlowInstanceData
            {
                Id = Id,
                Name = Name,
                Caller = Caller,
                FlowDefinitionId = flowDef.Id,
                ContextJson = Encoding.UTF8.GetBytes(Context.ToJson()),
                State = Context.ExecutionState,
                Start = Start,
                End = DateTimeOffset.UtcNow
            };
        }

        private async Task Stitch()
        {
            foreach (Activity activity in Flow.Activities)
            {
                // link previous 
                try
                {
                    string[] links = Flow.Links.Where(l => l.Destination == activity.Ref).Select(l => l.Source).ToArray();
                    activity.Previous = Flow.Activities.Where(a => links.Contains(a.Ref)).ToArray();
                }
                catch (Exception ex)
                {
                    await Log(WorkflowLogLevel.Error, $"Problem in previous activity selection for activity {activity.Ref}:\n{ex.Message}\n{ex.StackTrace}");
                }
            }

            foreach (Activity activity in Flow.Activities)
            {

                // link next
                try
                {
                    activity.Next = Flow.Activities.Where(a => a.Previous?.Contains(activity) ?? false).ToArray();
                }
                catch (Exception ex)
                {
                    await Log(WorkflowLogLevel.Error, $"Problem in next activity selection for activity {activity.Ref}:\n{ex.Message}\n{ex.StackTrace}");
                }

                // compile link code
                try
                {
                    activity.AssignCode = BuildAssign(activity, Flow);
                }
                catch (Exception ex)
                {
                    await Log(WorkflowLogLevel.Error, $"Problem in one or more links for activity {activity.Ref}:\n{ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        private static string BuildAssign(Activity activity, Flow flow)
        {
            string[] assigns = activity.Previous.Select(source =>
            {
                Link link = flow.Links.First(l => l.Source == source.Ref && l.Destination == activity.Ref);
                string sourceType = source.GetType().GetCSharpTypeName();
                string destType = activity.GetType().GetCSharpTypeName();
                return string.IsNullOrEmpty(link.Expression?.Trim())
                    ? null
                    : $"//LINK:: {source.Ref} => {activity.Ref}\n" + link.Expression
                        .Replace("destination.", $"(({destType})activity).")
                        .Replace("source.", $"flow.GetActivity<{sourceType}>(\"{source.Ref}\").");
            })
                .Where(i => i != null)
                .ToArray();

            string body = $"\t{string.Join(";\n\t", assigns)}";

            // the complete block as a single "Action<Activity>" that can be called
            return assigns.Any() ? $"(activity, variables, flow) => {{\n{body}\n}}" : null;
        }
    }
}