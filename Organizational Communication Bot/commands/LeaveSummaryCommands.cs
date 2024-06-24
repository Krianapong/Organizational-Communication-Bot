using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Organizational_Communication_Bot.commands
{
    internal class LeaveSummaryCommands : BaseCommandModule
    {
        private string connectionString = "Data Source=KIROV\\DATABASE64;Initial Catalog=LeaveRequests;Integrated Security=True;";

        [Command("LeaveCount")]
        [Description("แสดงจำนวนครั้งที่ลาด้วยเหตุผลที่กำหนด")]
        public async Task LeaveCount(CommandContext ctx, string username)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"SELECT COUNT(*) AS RequestCount
                               FROM LeaveRequests
                               WHERE Username = @Username";

                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Username", username);

                try
                {
                    await connection.OpenAsync();
                    object result = await command.ExecuteScalarAsync();

                    int count = Convert.ToInt32(result);
                    await ctx.Channel.SendMessageAsync($"{username} ได้ทำการลาทั้งหมด {count} ครั้ง");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await ctx.Channel.SendMessageAsync("เกิดข้อผิดพลาดในการดึงข้อมูล");
                }
            }
        }

        [Command("LeaveSummary")]
        [Description("สรุปการลาของพนักงานวันนี้")]
        public async Task LeaveSummary(CommandContext ctx)
        {
            var today = DateTime.Today;
            var summary = new StringBuilder($"สรุปการลาของพนักงานวันที่ {today.ToShortDateString()}:\n\n");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"SELECT Username, LeaveType, StartDate, EndDate, Reason 
                               FROM LeaveRequests 
                               WHERE StartDate = @Today AND EndDate = @Today";

                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Today", today);

                try
                {
                    await connection.OpenAsync();
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        string username = reader["Username"].ToString();
                        string leaveType = reader["LeaveType"].ToString();
                        DateTime startDate = (DateTime)reader["StartDate"];
                        DateTime endDate = (DateTime)reader["EndDate"];
                        string reason = reader["Reason"].ToString();

                        summary.AppendLine($"Username: {username}, Leave Type: {leaveType}, " +
                                           $"Start Date: {startDate.ToShortDateString()}, End Date: {endDate.ToShortDateString()}, " +
                                           $"Reason: {reason}");
                    }

                    reader.Close();

                    if (summary.Length == 0)
                    {
                        await ctx.Channel.SendMessageAsync("ไม่มีข้อมูลการลาในวันนี้");
                    }
                    else
                    {
                        await ctx.Channel.SendMessageAsync(summary.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await ctx.Channel.SendMessageAsync("เกิดข้อผิดพลาดในการดึงข้อมูล");
                }
            }
        }
    }
}
