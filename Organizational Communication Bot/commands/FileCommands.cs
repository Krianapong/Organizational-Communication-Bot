using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Organizational_Communication_Bot.commands
{
    public class FileCommands : ApplicationCommandModule
    {
        [SlashCommand("uploadfile", "อัปโหลดไฟล์และส่งกลับให้")]
        public async Task UploadFile(InteractionContext ctx)
        {
            // Ensure the user has attached a file
            if (!ctx.Interaction.Data.Options.Any(option => option.Name == "file"))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("กรุณาแนบไฟล์ที่ต้องการอัปโหลด"));
                return;
            }

            // Get the uploaded file option
            var fileOption = ctx.Interaction.Data.Options.FirstOrDefault(option => option.Name == "file");

            // Check if the file option contains the file
            if (fileOption.Type != ApplicationCommandOptionType.Attachment)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("ไม่พบไฟล์ที่แนบมา"));
                return;
            }

            // Check the file extension
            var fileName = fileOption.Name.ToLower();
            if (!fileName.EndsWith(".pdf") && !fileName.EndsWith(".png"))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("รองรับเฉพาะไฟล์ PDF และ PNG เท่านั้น"));
                return;
            }

            // Get the file content and file name
            var fileStream = fileOption.Value as Stream;

            // Save the file to a temporary location
            string tempFilePath = Path.Combine(Path.GetTempPath(), fileName);
            using (FileStream file = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                await fileStream.CopyToAsync(file);
            }

            // Send a response message
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent($"ได้รับไฟล์: {fileName}"));

            // Process the file (optional)
            // ... Add your file processing logic here ...
        }
    }
}
