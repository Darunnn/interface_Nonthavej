using System;
using System.Collections.Generic;
using System.IO;
using interface_Nonthavej.Models;

namespace interface_Nonthavej.FunctionFrom.Settings
{
    public class FnLogSettings
    {
        private readonly string _iniPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Config", "CleanOldLogs.ini");

        public LogSettings Load()
        {
            var settings = new LogSettings(); // default = 30 days

            if (!File.Exists(_iniPath))
                return settings;

            foreach (var line in File.ReadAllLines(_iniPath))
            {
                var parts = line.Split('=');
                if (parts.Length == 2 &&
                    parts[0].Trim().Equals("LogRetentionDays", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(parts[1].Trim(), out int days))
                        settings.LogRetentionDays = days;
                    break;
                }
            }

            return settings;
        }

        public void Save(LogSettings settings)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_iniPath));

            var lines = File.Exists(_iniPath)
                ? new List<string>(File.ReadAllLines(_iniPath))
                : new List<string>();

            bool found = false;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].TrimStart().StartsWith("LogRetentionDays", StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] = $"LogRetentionDays={settings.LogRetentionDays}";
                    found = true;
                    break;
                }
            }

            if (!found)
                lines.Add($"LogRetentionDays={settings.LogRetentionDays}");

            File.WriteAllLines(_iniPath, lines);
        }
    }
}