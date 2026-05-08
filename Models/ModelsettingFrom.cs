using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace interface_Nonthavej.Models
{
    public class APISettings
    {
        public string ApiEndpoint { get; set; } = string.Empty;
        public int ApiTimeoutSeconds { get; set; } = 30;
        public int ApiRetryAttempts { get; set; } = 3;
        public int ApiRetryDelaySeconds { get; set; } = 5;
    }
    public class DatabaseSettings
    {
        public string Server { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class ConnectionTestResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }
    public class LogSettings
    {
        public int LogRetentionDays { get; set; } = 30;
    }
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
    }
}
