using Core.Objects.Dtos.Workflow;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Core.Connectivity.Workflow.Discord
{
    public class DiscordSendAttachmentToChannelActivity : DiscordActivity
    {
        public ulong ChannelId { get; set; }
        public string Message { get; set; }
        public string FileName { get; set; }

        [IgnoreWhenFlowComplete]
        public Stream AttachmentStream { get; set; }

        public async override Task Execute()
        {
            await DiscordDo(async client =>
            {
                var channel = await client.GetChannelAsync(ChannelId);
                var messageBuilder = new DiscordMessageBuilder()
                    .WithContent(Message)
                    .WithFiles(new Dictionary<string, Stream>() { { FileName, AttachmentStream } })
                    .SendAsync(channel);

            });
        }
    }
}
