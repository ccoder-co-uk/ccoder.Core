using cCoder.Core.Objects.Dtos.Workflow;

namespace cCoder.Core.Objects.Workflow.Activities.DMS;

public class DMSCreateTextFilesActivity : DMSActivity
{

    public IEnumerable<string> Names { get; set; }

    [IgnoreWhenFlowComplete]
    public IEnumerable<string> Contents { get; set; }

    public override async Task Execute()
    {
        try
        {
            if (Names != null && Contents != null)
            {
                using HttpClient api = GetHttpClient();
                using IEnumerator<string> n = Names.GetEnumerator();
                using IEnumerator<string> c = Contents.GetEnumerator();
                while (n.MoveNext() && c.MoveNext())
                {
                    if (n.Current != null && c.Current != null)
                    {
                        _ = await api.PutAsync($"DMS/{Path.TrimEnd('/')}/{n.Current}", new StringContent(c.Current));
                    }
                }

                Log(WorkflowLogLevel.Info, $"File upload complete, {Names.Count()} files posted to DMS folder {Path}");
            }
            else
            {
                Log(WorkflowLogLevel.Warning, $"No files requested for creation.");
            }
        }
        catch (Exception ex)
        {
            Log(WorkflowLogLevel.Error, $"Failed to create DMS file because of exception:\n{ex.Message}");
        }
    }
}