using cCoder.Core.Objects.Workflow.Activities;
using Renci.SshNet;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace cCoder.Core.Connectivity.Workflow.Sftp
{
    // Abstract types should not have constructors
    // I know, but I don't care, this stuff is type heirarchy specific
    public abstract class SftpActivity : Activity
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        protected SftpActivity() { }

        public override Task Execute() => Task.FromResult(true);

        protected T SftpDo<T>(Func<SftpClient, T> operation)
        {
            ConnectionInfo connectionInfo = new(Host, Username, new PasswordAuthenticationMethod(Username, Password));
            SftpClient client = new(connectionInfo);

            try
            {
                client.Connect();
                Log(Objects.Dtos.Workflow.WorkflowLogLevel.Info, $"Connected to Server @ {Host} as User {Username}");
                return operation(client);
            }
            catch { State = ActivityState.Failed; }
            finally
            {
                if (client.IsConnected)
                {
                    client.Disconnect();
                }

                client.Dispose();
                Log(Objects.Dtos.Workflow.WorkflowLogLevel.Info, $"Disconnected from Server @ {Host}");
            }

            return default;
        }

        protected void SftpDo(Action<SftpClient> operation)
        {
            ConnectionInfo connectionInfo = new(Host, Username, new PasswordAuthenticationMethod(Username, Password));
            SftpClient client = new(connectionInfo);

            try
            {
                client.Connect();
                Log(Objects.Dtos.Workflow.WorkflowLogLevel.Info, $"Connected to Server @ {Host} as User {Username}");
                operation(client);
            }
            finally
            {
                if (client.IsConnected)
                {
                    client.Disconnect();
                }

                client.Dispose();
                Log(Objects.Dtos.Workflow.WorkflowLogLevel.Info, $"Disconnected from Server @ {Host}");

            }

        }

        protected void BuildPath(SftpClient client, Objects.Path folderPath)
        {
            if (folderPath.Length > 0)
            {
                Renci.SshNet.Sftp.SftpFile existingFolder = client.ListDirectory(string.Empty).FirstOrDefault(f => f.FullName.ToLower() == folderPath.Lowered);

                if (existingFolder == null)
                {
                    if (folderPath.ParentPath.Depth > 0)
                    {
                        BuildPath(client, folderPath.ParentPath);
                    }

                    Log(Objects.Dtos.Workflow.WorkflowLogLevel.Debug, "Building path: " + folderPath.FullPath);
                    if (!client.Exists(folderPath.FullPath))
                    {
                        client.CreateDirectory(folderPath.FullPath);
                    }
                }
            }
        }
    }
}