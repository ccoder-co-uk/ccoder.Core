using System.Threading.Tasks;

namespace Core.Connectivity.Workflow.Discord
{
    public class DiscordSendMessageToChannelActivity : DiscordActivity
    {
        public ulong ChannelId { get; set; }
        public string Message { get; set; }

        public async override Task Execute()
        {
            await DiscordDo(async client =>
            {
                var channel = await client.GetChannelAsync(ChannelId);
                await client.SendMessageAsync(channel, Message);
            });
        }
    }
}
