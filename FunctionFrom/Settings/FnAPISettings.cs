using System;
using System.IO;
using System.Text;
using interface_Nonthavej.Models;

namespace interface_Nonthavej.FunctionFrom.Settings
{
    /// <summary>
    /// จัดการการโหลดและบันทึกการตั้งค่า API
    /// </summary>
    public class FnAPISettings
    {
        private const string ConfigFolder = "Config";
        private const string ConfigFile = "appsettings.ini";

        /// <summary>
        /// โหลดการตั้งค่า API จากไฟล์
        /// </summary>
        public APISettings Load()
        {
            var settings = new APISettings();
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFolder, ConfigFile);

            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                    var parts = line.Split('=');
                    if (parts.Length != 2) continue;

                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    switch (key.ToUpper())
                    {
                        case "APIENDPOINT":
                            settings.ApiEndpoint = value;
                            break;
                        case "APITIMEOUTSECONDS":
                            if (int.TryParse(value, out int timeout))
                                settings.ApiTimeoutSeconds = timeout;
                            break;
                        case "APIRETRYATTEMPTS":
                            if (int.TryParse(value, out int retry))
                                settings.ApiRetryAttempts = retry;
                            break;
                        case "APIRETRYDELAYSECONDS":
                            if (int.TryParse(value, out int delay))
                                settings.ApiRetryDelaySeconds = delay;
                            break;
                    }
                }
            }

            return settings;
        }

        /// <summary>
        /// บันทึกการตั้งค่า API ลงไฟล์
        /// </summary>
        public void Save(APISettings settings)
        {
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFolder);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, ConfigFile);

            var content = new StringBuilder();
            content.AppendLine("# ===== API SETTINGS =====");
            content.AppendLine("# URL endpoint สำหรับส่งข้อมูลไป Drug Dispenser API");
            content.AppendLine("# Format: http://{HOST}/api/conHIS/insertPrescription");
            content.AppendLine($"ApiEndpoint={settings.ApiEndpoint}");
            content.AppendLine($"# API timeout in seconds (default: 30)");
            content.AppendLine($"ApiTimeoutSeconds={settings.ApiTimeoutSeconds}");
            content.AppendLine($"# API retry attempts when failed (default: 3)");
            content.AppendLine($"ApiRetryAttempts={settings.ApiRetryAttempts}");
            content.AppendLine($"# API retry delay in seconds (default: 5)");
            content.AppendLine($"ApiRetryDelaySeconds={settings.ApiRetryDelaySeconds}");
            content.AppendLine();
            content.AppendLine("# ===== PROCESSING SETTINGS =====");
            content.AppendLine("# ระยะเวลาในการตรวจสอบข้อมูลใหม่ (วินาที)");
            content.AppendLine("# ค่าที่แนะนำ: 15-60 วินาที");
            content.AppendLine("ProcessingIntervalSeconds=1");
            content.AppendLine("# จำนวนสูงสุดของ records ที่ประมวลผลในแต่ละรอบ");
            content.AppendLine("MaxProcessingBatchSize=50");
            content.AppendLine("# เริ่มการประมวลผลอัตโนมัติเมื่อเปิดโปรแกรม (true/false)");
            content.AppendLine("AutoStart=true");

            File.WriteAllText(path, content.ToString());
        }
    }
}


