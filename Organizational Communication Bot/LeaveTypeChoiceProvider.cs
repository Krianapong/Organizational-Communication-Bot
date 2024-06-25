using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Organizational_Communication_Bot.commands;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Organizational_Communication_Bot
{
    public class LeaveTypeChoiceProvider : IChoiceProvider
    {
        private readonly string connectionString = "Data Source=KIROV\\DATABASE64;Initial Catalog=LeaveRequests;Integrated Security=True;";

        public LeaveTypeChoiceProvider()
        {
        }

        public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
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

            return leaveTypes.Select(l => new DiscordApplicationCommandOptionChoice(l.LeaveTypeName, l.LeaveTypeName));
        }
    }
}
