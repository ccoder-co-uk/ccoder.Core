using Core.Objects.Dtos.Workflow;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Text;
using File = Core.Objects.Entities.DMS.File;

namespace Core.Objects.Workflow.Activities.DMS;

public class XmlFilteredFolderContentActivity : DMSActivity
{
    [IgnoreWhenFlowComplete]
    public string[] RawData { get; private set; }

    [IgnoreWhenFlowComplete]
    public IEnumerable<File> Files { get; set; }

    [JsonIgnore]
    public string Filter { get; set; } = "";

    [JsonIgnore]
    public dynamic[] ParsedData => RawData?.Select(i => Data.ParseXml<dynamic>(i)).ToArray();

    [JsonIgnore]
    public dynamic[] FlattenedData => ParsedData?.Select(i => Data.Flatten(i)).ToArray();

    public override async Task Execute()
    {
        using HttpClient api = GetHttpClient();

        api.Timeout = TimeSpan.FromMinutes(10);

        (string file, string xml)[] data = (await GetFilesWithContents(api)).ToArray();

        Files = data.Select(d => new File {
            Name = d.file,
            Path = $"{Path.Trim().TrimEnd("/".ToCharArray())}/{d.file}"
        });
        RawData = data.Select(d => d.xml).ToArray();
    }

    private string ConvertToString(byte[] raw) =>
        Encoding.UTF8.GetString(raw);

    protected async Task<IEnumerable<(string, string)>> GetFilesWithContents(HttpClient api)
    {
        var path = Path.Trim().TrimEnd("/".ToCharArray());

        using var result = await api.GetStreamAsync($"DMS/{path}?search={Filter}");
        using ZipArchive folderArchive = new(result);

        List<(string, string)> results = [];

        foreach (var entry in folderArchive.Entries)
            if (entry.Name.EndsWith(".xml"))
            {
                using Stream entryStream = entry.Open();
                using StreamReader entryReader = new(entryStream);
                results.Add((entry.Name, entryReader.ReadToEnd()));
            }

        return results;
    }

    string Now() => DateTimeOffset.UtcNow.ToString("HH:mm:ss");
}