using System.Threading.Tasks;

namespace cCoder.Core.Connectivity.Workflow.Sftp;

public class SftpMoveActivity : SftpActivity
{
    public string[] SourcePaths { get; set; }
    public string[] DestinationPaths { get; set; }

    public override Task Execute()
    {
        SftpDo(client =>
        {
            for (int i = 0; i < SourcePaths.Length; i++)
            {
                BuildPath(client, new Objects.Path(DestinationPaths[i]).ParentPath);
                client.RenameFile(SourcePaths[i], DestinationPaths[i]);
            }
        });

        return Task.FromResult(true);
    }
}