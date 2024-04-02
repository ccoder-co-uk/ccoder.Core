using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Core.Connectivity.Workflow.Sftp
{
    public class SftpCreateTextFilesActivity : SftpActivity
    {

        public string[] FullPaths { get; set; }


        public string[] Contents { get; set; }

        public override Task Execute()
        {
            SftpDo(client =>
            {
                for (int i = 0; i < FullPaths.Length; i++)
                {

                    BuildPath(client, new Objects.Path(FullPaths[i]).ParentPath);
                    MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(Contents[i]));
                    client.UploadFile(memoryStream, FullPaths[i]);
                    memoryStream.Dispose();
                }

                Log(Objects.Dtos.Workflow.WorkflowLogLevel.Info, "Upload Complete.");
            });

            return Task.FromResult(true);
        }
    }
}