using System;
using interface_Nonthavej.Models;

namespace interface_Nonthavej.FunctionFrom.Settings
{
    /// <summary>
    /// ตรวจสอบความถูกต้องของการตั้งค่าต่างๆ
    /// </summary>
    public class SettingsValidator
    {
        /// <summary>
        /// ตรวจสอบการตั้งค่าฐานข้อมูล
        /// </summary>
        public ValidationResult ValidateDatabase(DatabaseSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Server))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "กรุณาระบุ Server",
                    FieldName = "Server"
                };
            }

            if (string.IsNullOrWhiteSpace(settings.Database))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "กรุณาระบุ Database",
                    FieldName = "Database"
                };
            }

            if (string.IsNullOrWhiteSpace(settings.UserId))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "กรุณาระบุ User ID",
                    FieldName = "UserId"
                };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// ตรวจสอบการตั้งค่า API
        /// </summary>
        public ValidationResult ValidateAPI(APISettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.ApiEndpoint))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "กรุณาระบุ API Endpoint",
                    FieldName = "ApiEndpoint"
                };
            }

            if (!Uri.TryCreate(settings.ApiEndpoint, UriKind.Absolute, out _))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "API Endpoint ไม่ถูกต้อง\nกรุณาระบุ URL ที่ถูกต้อง (เช่น https://example.com/api)",
                    FieldName = "ApiEndpoint"
                };
            }

            if (settings.ApiTimeoutSeconds <= 0)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "API Timeout ต้องมากกว่า 0 วินาที",
                    FieldName = "ApiTimeout"
                };
            }

            if (settings.ApiRetryAttempts < 0)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "API Retry Attempts ต้องไม่ติดลบ",
                    FieldName = "ApiRetry"
                };
            }

            if (settings.ApiRetryDelaySeconds < 0)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "API Retry Delay ต้องไม่ติดลบ",
                    FieldName = "ApiRetryDelay"
                };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// ตรวจสอบการตั้งค่า Log
        /// </summary>
        public ValidationResult ValidateLog(LogSettings settings)
        {
            if (settings.LogRetentionDays <= 0)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Log Retention Days ต้องมากกว่า 0 วัน",
                    FieldName = "LogRetention"
                };
            }

            return new ValidationResult { IsValid = true };
        }
    }

    /// <summary>
    /// ผลลัพธ์การตรวจสอบความถูกต้อง
    /// </summary>
    
}