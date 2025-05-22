using cCoder.Core.Objects.Extensions;
using System.Net;
using System.Net.Http.Headers;

namespace cCoder.Core.Objects.Workflow.Activities.DMS;

public class FolderImportActivity : DMSActivity
{ 
    public string RemoteApiUrl { get; set; }
    public string RemoteAuthToken { get; set; }
    public string RemotePath { get; set; }

    public override async Task Execute()
    {
        await base.Execute();
        using HttpClient remoteApi = GetRemoteHttpClient();
        using HttpClient localApi = GetHttpClient();

        Log(Dtos.Workflow.WorkflowLogLevel.Info, $"Downloading from {RemoteApiUrl}DMS/{RemotePath}");
        HttpResponseMessage response = await remoteApi.GetAsync($"DMS/{RemotePath}", HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
            Log(Dtos.Workflow.WorkflowLogLevel.Warning, $"Source {RemoteApiUrl}DMS/{RemotePath} returned nothing downloadable:\n" + 
                $"HTTP Status: {response.StatusCode}:\n{await response.Content.ReadAsStringAsync()}");

            State = ActivityState.Skipped;
            return;
        }

        using Stream remoteFolderStream = await response.Content.ReadAsStreamAsync();
        MemoryStream memoryStream = new();
        await remoteFolderStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        Log(Dtos.Workflow.WorkflowLogLevel.Info, $"Importing to {localApi.BaseAddress}DMS/{Path}");
        response = await localApi.PostAsync($"DMS/{Path}?unpack=true&ignoreArchiveRoot=true", new StreamContent(memoryStream));

        if (!response.IsSuccessStatusCode)
        {
            Log(Dtos.Workflow.WorkflowLogLevel.Error, $"Failed to upload to {localApi.BaseAddress}DMS/{Path} due to error:\n" + 
                $"HTTP Status: {response.StatusCode}:\n{await response.Content.ReadAsStringAsync()}");

            State = ActivityState.Failed;
        }
    }

    protected HttpClient GetRemoteHttpClient()
    {
        HttpClient result = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
            .WithBaseUri(RemoteApiUrl);

        if (RemoteAuthToken != null)
            result.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", RemoteAuthToken);

        return result;
    }
}