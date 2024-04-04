using Newtonsoft.Json;
using Renci.SshNet.Sftp;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace cCoder.Core.Connectivity.Workflow.Sftp
{
    public class SftpFetchActivity : SftpActivity
    {

        public string Path { get; set; }

        [JsonIgnore]
        public string FileAsText
        {
            get
            {
                return SftpDo(client =>
                {
                    try
                    {
                        using SftpFileStream output = client.OpenRead(Path);
                        return new StreamReader(output).ReadToEnd();
                    }
                    catch { return null; }
                });
            }
        }

        [JsonIgnore]
        public string[] DirectoryContentsAsText
        {
            get
            {
                return SftpDo(client =>
                {
                    List<SftpFile> files = new();
                    foreach (SftpFile item in client.ListDirectory(Path))
                    {
                        if (!item.IsDirectory)
                        {
                            files.Add(item);
                        }
                    }

                    string[] result = files.Select(f =>
                    {
                        using StreamReader reader = new(client.OpenRead(f.FullName));
                        return reader.ReadToEnd();
                    })
                    .ToArray();

                    return result;
                });
            }
        }

        [JsonIgnore]
        public IEnumerable<SftpFile> FileList
        {
            get
            {
                return SftpDo(client =>
                {
                    List<SftpFile> result = new();
                    foreach (SftpFile item in client.ListDirectory(Path))
                    {
                        if (!item.IsDirectory)
                        {
                            result.Add(item);
                        }
                    }

                    return result;
                });
            }
        }

        public string[] FilesAsText(IEnumerable<SftpFile> files)
        {
            return SftpDo(client =>
            {
                return files != null
                    ? files.Select(f =>
                    {
                        Log(Objects.Dtos.Workflow.WorkflowLogLevel.Debug, "Downloading: " + f.FullName);
                        using StreamReader reader = new(client.OpenRead(f.FullName));
                        return reader.ReadToEnd();
                    })
                    .ToArray()
                    : System.Array.Empty<string>();
            });
        }
    }
}