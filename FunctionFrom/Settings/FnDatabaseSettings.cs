using Microsoft.Data.SqlClient;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using interface_Nonthavej.Models;

namespace interface_Nonthavej.FunctionFrom.Settings
{
    /// <summary>
    /// จัดการการโหลดและบันทึกการตั้งค่าฐานข้อมูล
    /// </summary>
    public class FnDatabaseSettings
    {
        private const string ConnFolder = "Connection";
        private const string ConnFile = "connectdatabase.ini";

        /// <summary>
        /// โหลดการตั้งค่าฐานข้อมูลจากไฟล์
        /// </summary>
        public DatabaseSettings Load()
        {
            var settings = new DatabaseSettings();
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConnFolder, ConnFile);

            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (line.StartsWith("Server="))
                        settings.Server = line.Replace("Server=", "").Trim().TrimEnd(';');
                    else if (line.StartsWith("Database="))
                        settings.Database = line.Replace("Database=", "").Trim().TrimEnd(';');
                    else if (line.StartsWith("User Id="))
                        settings.UserId = line.Replace("User Id=", "").Trim().TrimEnd(';');
                    else if (line.StartsWith("Password="))
                        settings.Password = line.Replace("Password=", "").Trim().TrimEnd(';');
                }
            }

            return settings;
        }

        /// <summary>
        /// บันทึกการตั้งค่าฐานข้อมูลลงไฟล์
        /// </summary>
        public void Save(DatabaseSettings settings)
        {
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConnFolder);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, ConnFile);

            var content = new StringBuilder();
            content.AppendLine($"Server={settings.Server};");
            content.AppendLine($"Database={settings.Database};");
            content.AppendLine($"User Id={settings.UserId};");
            content.AppendLine($"Password={settings.Password};");
            content.AppendLine($"TrustServerCertificate=True;");

            File.WriteAllText(path, content.ToString());
        }

        /// <summary>
        /// ทดสอบการเชื่อมต่อฐานข้อมูล
        /// </summary>
        public async Task<ConnectionTestResult> TestConnectionAsync(DatabaseSettings settings)
        {
            var result = new ConnectionTestResult();

            try
            {
                var connectionString = $"Server={settings.Server};" +
                                     $"Database={settings.Database};" +
                                     $"User Id={settings.UserId};" +
                                     $"Password={settings.Password};";

                await Task.Run(() =>
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        using (var cmd = new SqlCommand("SELECT @@VERSION", connection))
                        {
                            result.Version = cmd.ExecuteScalar()?.ToString() ?? "Unknown";
                        }
                    }
                });

                result.IsSuccess = true;
                result.Message = "Connection successful";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
            }

            return result;
        }
    }

  

   
}


