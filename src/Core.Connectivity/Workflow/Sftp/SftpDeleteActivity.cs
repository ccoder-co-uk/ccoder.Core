using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.Threading.Tasks;

namespace cCoder.Core.Connectivity.Workflow.Sftp
{
    public class SftpDeleteActivity : SftpActivity
    {
        public string[] Paths { get; set; }
        public string Result { get; set; }

        public override Task Execute()
        {
            SftpDo(client =>
            {
                for (int i = 0; i < Paths.Length; i++)
                {
                    if (!new Objects.Path(Paths[i]).IsToFile)
                    {
                        DeleteDirectory(client, Paths[i]);
                    }
                    else
                    {
                        client.DeleteFile(Paths[i]);
                    }
                }

                Result = "Success";
            });

            return Task.CompletedTask;
        }

        private static void DeleteDirectory(SftpClient client, string path)
        {
            System.Collections.Generic.IEnumerable<SftpFile> itemsInPath = client.ListDirectory(path);
            foreach (SftpFile file in itemsInPath)
            {
                if (file.Name is not "." and not "..")
                {
                    if (file.IsDirectory)
                    {
                        DeleteDirectory(client, file.FullName);
                    }
                    else
                    {
                        client.DeleteFile(file.FullName);
                    }
                }
            }

            client.DeleteDirectory(path);
        }
    }
}