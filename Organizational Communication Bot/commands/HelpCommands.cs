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

                // Replace this with your actual method to retrieve registered commands
                var registeredCommands = GetRegisteredCommands();

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

        // Method to retrieve registered commands (replace with your actual implementation)
        private List<DiscordApplicationCommand> GetRegisteredCommands()
        {
            // Implement logic to retrieve registered commands
            // This could involve accessing your bot's command handler or database
            // Return a list of DiscordApplicationCommand objects
            return null; // Replace with your actual logic
        }
    }
}
