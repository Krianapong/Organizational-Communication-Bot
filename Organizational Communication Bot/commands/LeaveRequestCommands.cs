using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Organizational_Communication_Bot.commands
{
    internal class LeaveRequestCommands : BaseCommandModule
    {
        private string connectionString = "Data Source=KIROV\\DATABASE64;Initial Catalog=LeaveRequests;Integrated Security=True;";
        private static IntroduceCommands introduceCommands = new IntroduceCommands();

        [Command("LeaveRequest")]
        [Description("บันทึกคำขอลา")]
        public async Task LeaveRequest(CommandContext ctx, string leaveType, [RemainingText] string reason)
        {
            DateTime currentDate = DateTime.Today;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"INSERT INTO LeaveRequests (Username, LeaveType, StartDate, EndDate, Reason)
                               VALUES (@Username, @LeaveType, @StartDate, @EndDate, @Reason)";

                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Username", ctx.User.Username);
                command.Parameters.AddWithValue("@LeaveType", leaveType);
                command.Parameters.AddWithValue("@StartDate", currentDate);
                command.Parameters.AddWithValue("@EndDate", currentDate);
                command.Parameters.AddWithValue("@Reason", reason);

                try
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    string introduction = await introduceCommands.GetUserIntroduction(ctx.User.Username);
                    await ctx.Channel.SendMessageAsync($"ได้รับคำขอลา {leaveType} จาก {introduction} ตั้งแต่ {currentDate.ToShortDateString()} ถึง {currentDate.ToShortDateString()} ด้วยเหตุผล: {reason}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await ctx.Channel.SendMessageAsync("เกิดข้อผิดพลาดในการบันทึกข้อมูล");
                }
            }
        }
    }
}
