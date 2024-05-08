using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Objects.Extensions;

using File = cCoder.Core.Objects.Entities.DMS.File;

namespace cCoder.Core.Objects.Workflow.Activities.DMS;

public abstract class DMSActivity : CoreActivity
{
    public string Path { get; set; }

    public override Task ExecuteInternal(IWorkflowContext context)
    {
        AppId = (int)context.Variables["AppId"];
        return base.ExecuteInternal(context);
    }

    protected async Task<File[]> GetFiles(HttpClient api) => 
        ParamsAllSet()
            ? (await api.GetODataCollection<Folder>($"Core/Folder?$filter=AppId eq {AppId} AND Path eq '{Path.Trim().TrimEnd("/".ToCharArray())}'&$expand=Files"))
                .FirstOrDefault()?
                .Files?
                .ToArray() ?? []
        : [];

    protected async Task<File> GetFile(HttpClient api)
        => ParamsAllSet()
            ? (await api.GetODataCollection<File>($"Core/File?$filter=Folder/AppId eq {AppId} AND Path eq '{Path.ToLower()}'")).FirstOrDefault()
            : null;

    protected async Task<string[]> GetFileContents(HttpClient api, IEnumerable<string> paths)
    {
        if (paths != null)
        {
            List<string> results = [];

            foreach (string f in paths)
                results.Add(await api.GetStringAsync($"DMS/{f.ToLower()}"));

            return [.. results];
        }
        else
        {
            Log(Dtos.Workflow.WorkflowLogLevel.Warning, "No File paths given to download.");
            return [];
        }
    }

    protected async Task<string> GetFileContents(HttpClient api)
    {
        if (ParamsAllSet())
        {
            try
            {
                Log(Dtos.Workflow.WorkflowLogLevel.Info, $"Fetching file @ ~DMS/{Path.ToLower()}");
                return await api.GetStringAsync($"DMS/{Path.ToLower()}");
            }
            catch { return string.Empty; }
        }
        else
            return string.Empty;
    }

    private bool ParamsAllSet()
    {
        bool result = true;

        if (AppId == 0)
        {
            Log(Dtos.Workflow.WorkflowLogLevel.Warning, $"  Unable to fetch file @ ~DMS/{Path ?? string.Empty} as the AppId has not been specified.");
            result = false;
        }

        if (Path == null || Path.Trim().TrimEnd("/".ToCharArray()).Length == 0)
        {
            Log(Dtos.Workflow.WorkflowLogLevel.Warning, $"  Unable to fetch file @ ~DMS/{Path ?? string.Empty} as the Path appears to be incorrect.");
            result = false;
        }

        return result;
    }
}