using System;
using System.Threading.Tasks;

namespace Core.Objects.Workflow.Activities.DMS
{
    public class MoveActivity : DMSActivity
    {
        public string OldPath { get; set; }

        public override async Task Execute()
        {
            using System.Net.Http.HttpClient api = GetHttpClient();
            System.Net.Http.HttpResponseMessage result = await api.PutAsync($"DMS/{OldPath}?moveTo={Path}", null);

            try { _ = result.EnsureSuccessStatusCode(); }
            catch (Exception ex)
            {
                Log(Dtos.Workflow.WorkflowLogLevel.Error, $"{ex.Message}\n{result.Content.ReadAsStringAsync()}");
                Log(Dtos.Workflow.WorkflowLogLevel.Error, "Paths in question are ...");
                Log(Dtos.Workflow.WorkflowLogLevel.Error, $"From: {OldPath}");
                Log(Dtos.Workflow.WorkflowLogLevel.Error, $"To  : {Path}");
            }
        }
    }

    public class MoveAllActivity : DMSActivity
    {

        public string[] OldPaths { get; set; }

        public override Task Execute()
        {
            using HttpClient api = GetHttpClient();

            foreach (string path in OldPaths)
            {
                Log(Dtos.Workflow.WorkflowLogLevel.Info, $"Moving file: {path} to {Path}...");
                Task<HttpResponseMessage> request = api.PutAsync($"DMS/{path}?moveTo={Path}", null);
                request.Wait();
                HttpResponseMessage result = request.Result;

                try
                {
                    _ = result.EnsureSuccessStatusCode();
                    Log(Dtos.Workflow.WorkflowLogLevel.Info, $"Moved file: {path} to {Path}");
                }
                catch (Exception ex)
                {
                    Log(Dtos.Workflow.WorkflowLogLevel.Warning, $"{ex.Message}\n{result.Content.ReadAsStringAsync()}");
                    Log(Dtos.Workflow.WorkflowLogLevel.Error, "Paths in question are ...");
                    Log(Dtos.Workflow.WorkflowLogLevel.Error, $"From: {path}");
                    Log(Dtos.Workflow.WorkflowLogLevel.Error, $"To  : {Path}");
                }
            }

            return Task.CompletedTask;
        }
    }
}