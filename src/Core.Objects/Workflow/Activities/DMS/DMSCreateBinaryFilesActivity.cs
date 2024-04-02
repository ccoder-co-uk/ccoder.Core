using Core.Objects.Dtos.Workflow;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Core.Objects.Workflow.Activities.DMS
{
    public class DMSCreateBinaryFilesActivity : DMSActivity
    {
        public IEnumerable<string> Names { get; set; }

        [IgnoreWhenFlowComplete]
        public IEnumerable<byte[]> Contents { get; set; }

        public override async Task Execute()
        {
            try
            {
                if (Names != null && Contents != null)
                {
                    using HttpClient api = GetHttpClient();
                    using IEnumerator<string> n = Names.GetEnumerator();
                    using IEnumerator<byte[]> c = Contents.GetEnumerator();
                    while (n.MoveNext() && c.MoveNext())
                    {
                        if (n.Current != null && c.Current != null)
                        {
                            _ = await api.PostAsync($"DMS/{Path.TrimEnd('/')}/{n.Current}", new ByteArrayContent(c.Current));
                        }
                    }

                    Log(WorkflowLogLevel.Info, $"File upload complete, files posted to DMS folder {Path}!");
                }
                else
                {
                    Log(WorkflowLogLevel.Warning, $"No files requested for creation.");
                }
            }
            catch (Exception ex)
            {
                Log(WorkflowLogLevel.Error, $"Failed to create DMS file because of exception:\n{ex.Message}");
            }
        }
    }
}