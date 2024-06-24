using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Text;
using System.Threading.Tasks;

namespace Organizational_Communication_Bot.commands
{
    internal class HelpCommands : BaseCommandModule
    {
        [Command("help")]
        [Description("แสดงรายการคำสั่งทั้งหมด")]
        public async Task Help(CommandContext ctx)
        {
            var helpMessage = new StringBuilder();
            helpMessage.AppendLine("คำสั่งที่สามารถใช้ได้:");

            foreach (var command in ctx.CommandsNext.RegisteredCommands.Values)
            {
                helpMessage.AppendLine($"{command.Name}: {command.Description}");
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = "รายการคำสั่งทั้งหมด",
                Description = helpMessage.ToString(),
                Color = DiscordColor.Azure
            };

            await ctx.Channel.SendMessageAsync(embed);
        }
    }
}
