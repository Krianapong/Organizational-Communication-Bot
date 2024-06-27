using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Organizational_Communication_Bot.commands
{
    public class HelpCommands : ApplicationCommandModule
    {
        [SlashCommand("helps", "แสดงรายการคำสั่งทั้งหมด")]
        public async Task Help(InteractionContext ctx)
        {
            try
            {
                var helpMessage = new StringBuilder();
                helpMessage.AppendLine("คำสั่งที่สามารถใช้ได้:");

                // Retrieve registered commands
                var registeredCommands = await GetRegisteredCommands(ctx.Client, ctx.Guild.Id);

                if (registeredCommands == null || registeredCommands.Count == 0)
                {
                    helpMessage.AppendLine("ไม่พบคำสั่งที่ลงทะเบียน");
                }
                else
                {
                    foreach (var command in registeredCommands)
                    {
                        helpMessage.AppendLine($"/{command.Name}: {command.Description}");
                    }
                }

                var embed = new DiscordEmbedBuilder
                {
                    Title = "รายการคำสั่งทั้งหมด",
                    Description = helpMessage.ToString(),
                    Color = new DiscordColor(0x007FFF) // Azure color
                };

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Help command: {ex.Message}");
                await ctx.CreateResponseAsync("เกิดข้อผิดพลาดในการดึงรายการคำสั่ง");
            }
        }

        // Method to retrieve registered commands
        private async Task<IReadOnlyList<DiscordApplicationCommand>> GetRegisteredCommands(DiscordClient client, ulong guildId)
        {
            // Fetch the commands from the Discord client
            var guildCommands = await client.GetGuildApplicationCommandsAsync(guildId);
            var globalCommands = await client.GetGlobalApplicationCommandsAsync();

            // Combine the two lists of commands
            var allCommands = new List<DiscordApplicationCommand>();
            allCommands.AddRange(guildCommands);
            allCommands.AddRange(globalCommands);

            return allCommands;
        }
    }
}
