using cCoder.Core.Objects.Dtos.Workflow;
using cCoder.Core.Objects.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using Workflow.Framework;

namespace Workflow
{
    public static class Functions
    {
        [FunctionName("Execute")]
        public static async Task Execute([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            FlowRunner runner = new();

            try
            {
                string json = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonConvert.DeserializeObject<WorkflowRequest>(json, ObjectExtensions.GetJSONSettings());
                await runner.Run(request);
            }
            catch (Exception ex)
            {
                await runner.Log(WorkflowLogLevel.Error, ex.Message, Guid.Empty);
                throw;
            }
        }

        [FunctionName("ExecuteScript")]
        public static async Task<object> ExecuteScript([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            try
            {
                string code = await new StreamReader(req.Body).ReadToEndAsync();
                ScriptRunner runner = new((WorkflowLogLevel level, string message) => Task.CompletedTask);
                object result = await runner.Run<object>(code, cCoder.Core.Objects.Workflow.Activities.Activity.ScriptImports);
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}