using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Organizational_Communication_Bot.commands
{
    public class HolydayCommands : ApplicationCommandModule
    {
        private readonly string connectionString = "Data Source=KIROV\\DATABASE64;Initial Catalog=LeaveRequests;Integrated Security=True;";

        [SlashCommand("Holyday", "วันหยุดประจำปีของบริษัท")]
        public async Task HolydayCommand(InteractionContext ctx,
                                         [Option("ปี", "ปีที่ต้องการดูวันหยุดประจำ")] int year = 0)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT HolidayMessage, HolidayYear FROM AnnualHolidays WHERE HolidayYear = @year";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@year", year);
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    string message;
                    if (reader.HasRows)
                    {
                        message = $"วันหยุดประจำปี {year} ของบริษัท:\n";
                        while (reader.Read())
                        {
                            string holidayMessage = reader.GetString(0);
                            message += $"- {holidayMessage}\n";
                        }
                    }
                    else
                    {
                        message = $"ไม่พบข้อมูลวันหยุดประจำปี {year} ของบริษัท";
                    }

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(message));
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"เกิดข้อผิดพลาดในการดึงข้อมูล: {ex.Message}"));
            }
        }
    }
}
