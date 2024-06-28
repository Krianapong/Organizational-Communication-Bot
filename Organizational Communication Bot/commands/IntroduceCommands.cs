using DSharpPlus.SlashCommands;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Organizational_Communication_Bot.commands
{
    public class IntroduceCommands : ApplicationCommandModule
    {
        private string connectionString = "Data Source=KIROV\\DATABASE64;Initial Catalog=LeaveRequests;Integrated Security=True;";

        public void WriteName()
        {
            System.Console.WriteLine();
        }

        [SlashCommand("introduce", "แนะนำตัวผู้ใช้")]
        public async Task Introduce(InteractionContext ctx,
                                    [Option("introduction", "คำแนะนำตัว")] string introduction)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"IF EXISTS (SELECT 1 FROM Users WHERE Username = @Username)
                               BEGIN
                                   UPDATE Users SET Introduction = @Introduction WHERE Username = @Username
                               END
                               ELSE
                               BEGIN
                                   INSERT INTO Users (Username, Introduction) VALUES (@Username, @Introduction)
                               END";

                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Username", ctx.User.Username);
                command.Parameters.AddWithValue("@Introduction", introduction);

                try
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    await ctx.CreateResponseAsync("การแนะนำตัวของคุณถูกบันทึกแล้ว!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await ctx.CreateResponseAsync("เกิดข้อผิดพลาดในการบันทึกข้อมูลการแนะนำตัว");
                }
            }
        }

        public async Task<string> GetUserIntroduction(string username)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT Introduction FROM Users WHERE Username = @Username";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Username", username);

                try
                {
                    await connection.OpenAsync();
                    object result = await command.ExecuteScalarAsync();
                    return result != null ? result.ToString() : username;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return username;
                }
            }
        }
    }
}
