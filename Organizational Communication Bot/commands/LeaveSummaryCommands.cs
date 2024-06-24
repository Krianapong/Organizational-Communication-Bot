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
        [Description("สรุปการลาของพนักงานตามเดือนและปีที่กำหนด")]
        
        public async Task LeaveSummary(CommandContext ctx, int month, int year)
        {
            if (month < 1 || month > 12)
            {
                await ctx.Channel.SendMessageAsync("เดือนที่ระบุไม่ถูกต้อง");
                return;
            }

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1); 

            var summary = new StringBuilder($"สรุปการลาของพนักงานเดือน {startDate.Month}/{startDate.Year}:\n\n");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"SELECT Username, LeaveType, StartDate, EndDate, Reason 
                               FROM LeaveRequests 
                               WHERE StartDate >= @StartDate AND EndDate <= @EndDate";

                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);

                try
                {
                    await connection.OpenAsync();
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        string username = reader["Username"].ToString();
                        string leaveType = reader["LeaveType"].ToString();
                        DateTime startLeaveDate = (DateTime)reader["StartDate"];
                        DateTime endLeaveDate = (DateTime)reader["EndDate"];
                        string reason = reader["Reason"].ToString();

                        summary.AppendLine($"Username: {username}, {leaveType}, " +
                                           $"{startLeaveDate.ToShortDateString()} - {endLeaveDate.ToShortDateString()} " +
                                           $"{reason}");
                    }

                    reader.Close();

                    if (summary.Length == 0)
                    {
                        await ctx.Channel.SendMessageAsync("ไม่มีข้อมูลการลาในเดือนและปีที่กำหนด");
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
