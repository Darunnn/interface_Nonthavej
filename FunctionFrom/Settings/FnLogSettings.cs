using System.Configuration;
using interface_Nonthavej.Models;

namespace interface_Nonthavej.FunctionFrom.Settings
{
    /// <summary>
    /// จัดการการโหลดและบันทึกการตั้งค่า Log
    /// </summary>
    public class FnLogSettings
    {
        /// <summary>
        /// โหลดการตั้งค่า Log จาก App.config
        /// </summary>
        public LogSettings Load()
        {
            var settings = new LogSettings();

            var logDays = ConfigurationManager.AppSettings["LogRetentionDays"];
            if (!string.IsNullOrEmpty(logDays) && int.TryParse(logDays, out int days))
            {
                settings.LogRetentionDays = days;
            }

            return settings;
        }

        /// <summary>
        /// บันทึกการตั้งค่า Log ลง App.config
        /// </summary>
        public void Save(LogSettings settings)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings["LogRetentionDays"] == null)
            {
                config.AppSettings.Settings.Add("LogRetentionDays", settings.LogRetentionDays.ToString());
            }
            else
            {
                config.AppSettings.Settings["LogRetentionDays"].Value = settings.LogRetentionDays.ToString();
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }

   
    
}


