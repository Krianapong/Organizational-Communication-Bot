using DSharpPlus;
using DSharpPlus.CommandsNext;
using Organizational_Communication_Bot.commands;
using Organizational_Communication_Bot.config;
using System;
using System.Threading.Tasks;

namespace Organizational_Communication_Bot
{
    internal class Program
    {
        private static DiscordClient Client { get; set; }

        private static CommandsNextExtension Commands { get; set; }

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

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { jsonReader.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            // Registering the commands from separate classes
            Commands.RegisterCommands<IntroduceCommands>();
            Commands.RegisterCommands<LeaveRequestCommands>();
            Commands.RegisterCommands<LeaveSummaryCommands>();
            Commands.RegisterCommands<HelpCommands>();

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
