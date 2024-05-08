using System.IO;
using System.Threading.Tasks;

namespace cCoder.Core.Connectivity.Workflow.Sftp;

public class SftpCreateBinaryFilesActivity : SftpActivity
{

    public string[] FullPaths { get; set; }


    public byte[] Contents { get; set; }

    public override Task Execute()
    {
        SftpDo(client =>
        {
            for (int i = 0; i < FullPaths.Length; i++)
            {
                BuildPath(client, new Objects.Path(FullPaths[i]).ParentPath);
                MemoryStream memoryStream = new(Contents[i]);
                client.UploadFile(memoryStream, FullPaths[i]);
                memoryStream.Dispose();
            }

            Log(Objects.Dtos.Workflow.WorkflowLogLevel.Info, "Upload Complete.");
        });

        return Task.FromResult(true);
    }
}