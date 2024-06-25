using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Organizational_Communication_Bot.commands
{
    public class LeaveTypeData
    {
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; }
    }

    public class LeaveRequestCommands : ApplicationCommandModule
    {
        private readonly string connectionString = "Data Source=KIROV\\DATABASE64;Initial Catalog=LeaveRequests;Integrated Security=True;";

        // โหลดข้อมูลประเภทการลาจากฐานข้อมูล
        private async Task<List<LeaveTypeData>> LoadLeaveTypesFromDatabase()
        {
            List<LeaveTypeData> leaveTypes = new List<LeaveTypeData>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = "SELECT LeaveTypeId, LeaveTypeName FROM LeaveTypes WHERE IsActive = 1"; // เลือกประเภทการลาที่ IsActive เป็น 1 (active)

                    SqlCommand command = new SqlCommand(sql, connection);

                    await connection.OpenAsync();

                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        int leaveTypeId = Convert.ToInt32(reader["LeaveTypeId"]);
                        string leaveTypeName = reader["LeaveTypeName"].ToString();

                        LeaveTypeData leaveType = new LeaveTypeData
                        {
                            LeaveTypeId = leaveTypeId,
                            LeaveTypeName = leaveTypeName
                        };

                        leaveTypes.Add(leaveType);
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading LeaveTypes from database: {ex.Message}");
            }

            return leaveTypes;
        }

        [SlashCommand("leaverequest", "บันทึกคำขอลา")]
        public async Task LeaveRequest(InteractionContext ctx,
                                       [ChoiceProvider(typeof(LeaveTypeChoiceProvider))][Option("ประเภทการลา", "ประเภทการลา")] string leaveTypeName,
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

                // โหลดข้อมูลประเภทการลาจากฐานข้อมูล
                List<LeaveTypeData> leaveTypes = await LoadLeaveTypesFromDatabase();

                // ตรวจสอบว่าประเภทการลาที่ร้องขออยู่ในฐานข้อมูลหรือไม่
                var selectedLeaveType = leaveTypes.FirstOrDefault(l => l.LeaveTypeName.Equals(leaveTypeName, StringComparison.OrdinalIgnoreCase));

                if (selectedLeaveType == null)
                {
                    await ctx.CreateResponseAsync($"ประเภทการลา '{leaveTypeName}' ไม่ถูกต้อง");
                    return;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = @"INSERT INTO LeaveRequests (DiscordUserId, LeaveTypeId, StartDate, EndDate, Reason)
                               VALUES (@DiscordUserId, @LeaveTypeId, @StartDate, @EndDate, @Reason)";

                    SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@DiscordUserId", (long)ctx.User.Id);
                    command.Parameters.AddWithValue("@LeaveTypeId", selectedLeaveType.LeaveTypeId);
                    command.Parameters.AddWithValue("@StartDate", startDateParsed);
                    command.Parameters.AddWithValue("@EndDate", endDateParsed);
                    command.Parameters.AddWithValue("@Reason", reason);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    await ctx.CreateResponseAsync($"ได้รับคำขอลา '{leaveTypeName}' จาก {ctx.User.Username} ตั้งแต่ {startDateParsed.ToShortDateString()} ถึง {endDateParsed.ToShortDateString()} ด้วยเหตุผล: {reason}");
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
