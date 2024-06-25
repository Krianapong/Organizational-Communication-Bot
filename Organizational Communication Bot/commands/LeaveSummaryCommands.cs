using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Organizational_Communication_Bot.commands
{
    public class LeaveSummaryCommands : ApplicationCommandModule
    {
        private string connectionString = "Data Source=KIROV\\DATABASE64;Initial Catalog=LeaveRequests;Integrated Security=True;";

        [SlashCommand("leavesummary", "สรุปการลาของพนักงานตามช่วงเวลาที่กำหนดหรือสำหรับวันปัจจุบัน")]
        public async Task LeaveSummary(InteractionContext ctx,
                                       [Option("month", "เดือน")] long? month = null,
                                       [Option("year", "ปี")] long? year = null)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime currentDate = DateTime.Today;

            var summary = new StringBuilder("สรุปการลาของพนักงาน:\n\n");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT u.Username, u.Introduction, lr.LeaveType, lr.StartDate, lr.EndDate, lr.Reason " +
                             "FROM LeaveRequests lr " +
                             "INNER JOIN Users u ON lr.DiscordUserId = u.DiscordUserId ";
                List<SqlParameter> parameters = new List<SqlParameter>();

                // Check if either month or year is specified
                if (month.HasValue || year.HasValue)
                {
                    sql += "WHERE ";

                    // Validate month range (1-12)
                    if (month.HasValue && (month < 1 || month > 12))
                    {
                        await ctx.CreateResponseAsync("กรุณาระบุเดือนที่ถูกต้อง (1-12)");
                        return;
                    }

                    if (month.HasValue)
                    {
                        sql += "MONTH(CONVERT(datetime, lr.StartDate, 101)) = @Month ";
                        parameters.Add(new SqlParameter("@Month", month.Value));
                    }

                    if (year.HasValue)
                    {
                        if (month.HasValue)
                            sql += "AND ";

                        sql += "YEAR(CONVERT(datetime, lr.StartDate, 101)) = @Year ";
                        parameters.Add(new SqlParameter("@Year", year.Value));
                    }
                }
                else
                {
                    // If no month or year specified, use current date
                    sql += "WHERE CONVERT(date, lr.StartDate) <= @CurrentDate AND CONVERT(date, lr.EndDate) >= @CurrentDate";
                    parameters.Add(new SqlParameter("@CurrentDate", currentDate));
                }

                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddRange(parameters.ToArray());

                try
                {
                    await connection.OpenAsync();
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    bool hasData = false;
                    var usersSummary = new Dictionary<string, StringBuilder>();

                    while (reader.Read())
                    {
                        string username = reader["Username"].ToString();
                        string introduction = reader["Introduction"]?.ToString() ?? "ไม่มีการแนะนำตัว";
                        string leaveType = reader["LeaveType"].ToString();
                        DateTime startLeaveDate = (DateTime)reader["StartDate"];
                        DateTime endLeaveDate = (DateTime)reader["EndDate"];
                        string reason = reader["Reason"].ToString();

                        if (!usersSummary.ContainsKey(username))
                        {
                            usersSummary[username] = new StringBuilder($"- Username: {username}\n  Introduction: {introduction}\n");
                        }

                        usersSummary[username].AppendLine($"  {leaveType}, {startLeaveDate.ToShortDateString()} - {endLeaveDate.ToShortDateString()}\n  เหตุผล: {reason}\n");

                        hasData = true;
                    }

                    reader.Close();

                    if (!hasData)
                    {
                        await ctx.CreateResponseAsync("ไม่มีข้อมูลการลาในช่วงเวลาที่กำหนด");
                    }
                    else
                    {
                        foreach (var userSummary in usersSummary.Values)
                        {
                            summary.AppendLine(userSummary.ToString());
                        }

                        var embed = new DiscordEmbedBuilder
                        {
                            Title = "สรุปการลาของพนักงาน",
                            Description = summary.ToString(),
                            Color = new DiscordColor(0x007FFF) // Azure color
                        };

                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
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
