using cCoder.Core.Objects.Workflow.Activities;
using DSharpPlus;
using System;
using System.Threading.Tasks;

namespace cCoder.Core.Connectivity.Workflow.Discord
{
    public class DiscordActivity : Activity
    {
        public string Token { get; set; }

        protected async ValueTask DiscordDo(Func<DiscordClient, ValueTask> action)
        {
            DiscordConfiguration configuration = new DiscordConfiguration
            {
                Token = Token,
                TokenType = TokenType.Bot
            };

            DiscordClient client = new DiscordClient(configuration);
            await client.ConnectAsync();
            await action(client);
        }

        public override Task Execute() => Task.FromResult(true);
    }
}
