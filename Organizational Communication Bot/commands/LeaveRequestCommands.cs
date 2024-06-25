using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Organizational_Communication_Bot.commands
{
    public enum LeaveType
    {
        ลากิจ,
        ลาป่วย,
        ลาพักร้อน
    }

    public class LeaveRequestCommands : ApplicationCommandModule
    {
        private readonly string connectionString = "Data Source=KIROV\\DATABASE64;Initial Catalog=LeaveRequests;Integrated Security=True;";

        [SlashCommand("leaverequest", "บันทึกคำขอลา")]
        public async Task LeaveRequest(InteractionContext ctx,
                                       [Option("ประเภทการลา", "ประเภทการลา")] LeaveType leaveType,
                                       [Option("เหตุผลการลา", "เหตุผลการลา")] string reason,
                                       [Option("วันที่เริ่มลา", "วันที่เริ่มลา (dd/MM/yyyy)", true)] string startDate = null,
                                       [Option("วันที่สิ้นสุดการลา", "วันที่สิ้นสุดการลา (dd/MM/yyyy)", true)] string endDate = null)
        {
            DateTime startDateParsed;
            DateTime endDateParsed;

            if (string.IsNullOrEmpty(startDate))
            {
                startDateParsed = DateTime.Today;
            }
            else if (!DateTime.TryParseExact(startDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out startDateParsed))
            {
                await ctx.CreateResponseAsync("รูปแบบวันที่เริ่มลาไม่ถูกต้อง (กรุณาใช้รูปแบบ dd/MM/yyyy)");
                return;
            }

            if (string.IsNullOrEmpty(endDate))
            {
                endDateParsed = DateTime.Today;
            }
            else if (!DateTime.TryParseExact(endDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out endDateParsed))
            {
                await ctx.CreateResponseAsync("รูปแบบวันที่สิ้นสุดการลาไม่ถูกต้อง (กรุณาใช้รูปแบบ dd/MM/yyyy)");
                return;
            }

            if (startDateParsed > endDateParsed)
            {
                await ctx.CreateResponseAsync("วันที่สิ้นสุดการลาต้องไม่อยู่ก่อนวันที่เริ่มลา");
                return;
            }

            try
            {
                await EnsureUserExists(ctx.User.Id, ctx.User.Username);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = @"INSERT INTO LeaveRequests (DiscordUserId, LeaveType, StartDate, EndDate, Reason)
                                   VALUES (@DiscordUserId, @LeaveType, @StartDate, @EndDate, @Reason)";

                    SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@DiscordUserId", (long)ctx.User.Id);
                    command.Parameters.AddWithValue("@LeaveType", leaveType.ToString());
                    command.Parameters.AddWithValue("@StartDate", startDateParsed);
                    command.Parameters.AddWithValue("@EndDate", endDateParsed);
                    command.Parameters.AddWithValue("@Reason", reason);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    await ctx.CreateResponseAsync($"ได้รับคำขอลา {leaveType} จาก {ctx.User.Username} ตั้งแต่ {startDateParsed.ToShortDateString()} ถึง {endDateParsed.ToShortDateString()} ด้วยเหตุผล: {reason}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await ctx.CreateResponseAsync("เกิดข้อผิดพลาดในการบันทึกข้อมูล");
            }
        }

        private async Task EnsureUserExists(ulong discordUserId, string username)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlCheck = "SELECT DiscordUserId FROM Users WHERE DiscordUserId = @DiscordUserId";
                SqlCommand commandCheck = new SqlCommand(sqlCheck, connection);
                commandCheck.Parameters.AddWithValue("@DiscordUserId", (long)discordUserId);

                try
                {
                    await connection.OpenAsync();

                    var result = await commandCheck.ExecuteScalarAsync();

                    if (result == null || result == DBNull.Value)
                    {
                        string sqlInsert = "INSERT INTO Users (DiscordUserId, Username) VALUES (@DiscordUserId, @Username)";
                        SqlCommand commandInsert = new SqlCommand(sqlInsert, connection);
                        commandInsert.Parameters.AddWithValue("@DiscordUserId", (long)discordUserId);
                        commandInsert.Parameters.AddWithValue("@Username", username);

                        await commandInsert.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }
    }
}
