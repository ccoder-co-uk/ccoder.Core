using cCoder.Core.Objects.Dtos.Workflow;
using cCoder.Core.Objects.Extensions;
using Newtonsoft.Json;
using System.Text;

namespace cCoder.Core.Objects.Workflow.Activities.DMS;

public class JsonFolderContentActivity : DMSActivity
{
    [IgnoreWhenFlowComplete]
    public string[] RawData { get; private set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }

    [JsonIgnore]
    public dynamic[] ParsedData => RawData?.Select(i => Data.ParseJson<dynamic>(i)).ToArray() ?? Array.Empty<dynamic>();

    [JsonIgnore]
    public dynamic[] FlattenedData => ParsedData?.Select(i => Data.Flatten(i)).ToArray() ?? Array.Empty<dynamic>();

    [IgnoreWhenFlowComplete]
    public Entities.DMS.File[] Files { get; set; }

    public override async Task Execute()
    {
        using HttpClient api = GetHttpClient();

        Files = (await GetFilesWithContents(api)).ToArray();
        RawData = Files.Select(f => ConvertToString(f.Contents.OrderByDescending(c => c.Version).FirstOrDefault()?.RawData)).ToArray();
    }

    protected async Task<IEnumerable<Entities.DMS.File>> GetFilesWithContents(HttpClient api)
    {
        string query = $"Core/File?$filter=Folder/AppId eq {AppId} AND Folder/Path eq '{Path.Trim().TrimEnd("/".ToCharArray())}' AND endswith(Name, '.json')&$expand=Contents";

        if (Page != null && PageSize != null)
            query += $"&$top={PageSize}&$skip={(Page - 1) * PageSize}";

        return await api.GetODataCollection<Entities.DMS.File>(query);
    }

    private string ConvertToString(byte[] raw) =>
        Encoding.UTF8.GetString(raw);
}