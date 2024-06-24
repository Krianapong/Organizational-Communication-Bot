using DSharpPlus;
using DSharpPlus.SlashCommands;
using Organizational_Communication_Bot.commands;
using Organizational_Communication_Bot.config;
using System;
using System.Threading.Tasks;

namespace Organizational_Communication_Bot
{
    internal class Program
    {
        private static DiscordClient Client { get; set; }

        static async Task Main(string[] args)
        {
            var jsonReader = new JSONReader();
            await jsonReader.ReadJSON();

            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = jsonReader.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            Client = new DiscordClient(discordConfig);

            Client.Ready += Client_Ready;

            var slash = Client.UseSlashCommands();
            slash.RegisterCommands<LeaveRequestCommands>();
            slash.RegisterCommands<LeaveSummaryCommands>();
            slash.RegisterCommands<IntroduceCommands>();
            slash.RegisterCommands<HelpCommands>();
            slash.RegisterCommands<FileCommands>();

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
        {
            Console.WriteLine("Bot is ready!");
            return Task.CompletedTask;
        }
    }
}
