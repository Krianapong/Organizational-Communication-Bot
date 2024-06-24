using DSharpPlus.SlashCommands;
using System;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Organizational_Communication_Bot.commands
{
    public class LeaveSlashCommands : ApplicationCommandModule
    {
        private string connectionString = "Data Source=KIROV\\DATABASE64;Initial Catalog=LeaveRequests;Integrated Security=True;";

        [SlashCommand("leavesummary", "สรุปการลาของพนักงานตามช่วงเวลาที่กำหนด")]
        public async Task LeaveSummary(InteractionContext ctx,
                                       [Option("month", "เดือน")] long? month = null,
                                       [Option("year", "ปี")] long? year = null)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (month.HasValue && year.HasValue)
            {
                startDate = new DateTime((int)year.Value, (int)month.Value, 1);
                endDate = startDate.Value.AddMonths(1).AddDays(-1);
            }
            else if (year.HasValue)
            {
                startDate = new DateTime((int)year.Value, 1, 1);
                endDate = new DateTime((int)year.Value, 12, 31);
            }

            var summary = new StringBuilder("สรุปการลาของพนักงาน:\n\n");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"SELECT Users.Username, LeaveRequests.LeaveType, LeaveRequests.StartDate, LeaveRequests.EndDate, LeaveRequests.Reason 
                               FROM LeaveRequests
                               INNER JOIN Users ON LeaveRequests.UserId = Users.Id";

                if (startDate.HasValue && endDate.HasValue)
                {
                    sql += " WHERE LeaveRequests.StartDate >= @StartDate AND LeaveRequests.EndDate <= @EndDate";
                }

                SqlCommand command = new SqlCommand(sql, connection);

                if (startDate.HasValue && endDate.HasValue)
                {
                    command.Parameters.AddWithValue("@StartDate", startDate.Value);
                    command.Parameters.AddWithValue("@EndDate", endDate.Value);
                }

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
                        await ctx.CreateResponseAsync("ไม่มีข้อมูลการลาในช่วงเวลาที่กำหนด");
                    }
                    else
                    {
                        await ctx.CreateResponseAsync(summary.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await ctx.CreateResponseAsync("เกิดข้อผิดพลาดในการดึงข้อมูล");
                }
            }
        }
    }
}
