using Microsoft.Data.SqlClient;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using interface_Nonthavej.Utils;

namespace interface_Nonthavej.Database
{
    /// <summary>
    /// จัดการ Database Connection Pool พร้อม Circuit Breaker Pattern
    /// </summary>
    public class DatabaseConnectionPool : IDisposable
    {
        private readonly string _connectionString;
        private readonly LogManager _logger;
        private readonly SemaphoreSlim _connectionSemaphore;
        private readonly CircuitBreaker _circuitBreaker;

        // Connection Pool Configuration
        private const int MaxPoolSize = 100;
        private const int MinPoolSize = 5;
        private const int ConnectionTimeout = 30; // seconds
        private const int CommandTimeout = 30; // seconds

        // Circuit Breaker Configuration
        private const int FailureThreshold = 5; // จำนวนครั้งที่ล้มเหลวก่อนเปิด Circuit
        private const int SuccessThreshold = 2; // จำนวนครั้งที่สำเร็จก่อนปิด Circuit
        private const int TimeoutInSeconds = 60; // เวลารอก่อนลอง Half-Open

        private bool _disposed = false;

        public DatabaseConnectionPool(string connectionString, LogManager logger)
        {
            _connectionString = BuildOptimizedConnectionString(connectionString);
            _logger = logger;
            _connectionSemaphore = new SemaphoreSlim(MaxPoolSize, MaxPoolSize);
            _circuitBreaker = new CircuitBreaker(
                FailureThreshold,
                SuccessThreshold,
                TimeSpan.FromSeconds(TimeoutInSeconds),
                logger
            );

            _logger?.LogInfo($"📊 Connection Pool initialized - Min: {MinPoolSize}, Max: {MaxPoolSize}");
        }

        /// <summary>
        /// สร้าง Connection String ที่ optimize แล้วสำหรับ Connection Pooling
        /// </summary>
        private string BuildOptimizedConnectionString(string baseConnectionString)
        {
            var builder = new SqlConnectionStringBuilder(baseConnectionString)
            {
                // Connection Pooling Settings
                Pooling = true,
                MinPoolSize = MinPoolSize,
                MaxPoolSize = MaxPoolSize,

                // Performance Settings
                ConnectTimeout = ConnectionTimeout,

                // Security & Reliability
                Encrypt = false, // ปรับตามความต้องการ
                TrustServerCertificate = true,

                // Connection Resiliency
                ConnectRetryCount = 3,
                ConnectRetryInterval = 10,

                // Performance Optimization
                MultipleActiveResultSets = false, // ปิดเพื่อประสิทธิภาพ
                ApplicationName = "interface_Nonthavej"
            };

            return builder.ConnectionString;
        }

        /// <summary>
        /// ดึง Connection จาก Pool พร้อม Circuit Breaker Protection
        /// </summary>
        public async Task<SqlConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
        {
            // ตรวจสอบ Circuit Breaker ก่อน
            if (_circuitBreaker.State == CircuitBreakerState.Open)
            {
                _logger?.LogWarning("⚠️ Circuit is OPEN - Request rejected");
                throw new InvalidOperationException("Circuit breaker is OPEN. Database might be unavailable.");
            }

            // รอ Semaphore เพื่อจำกัดจำนวน connection
            await _connectionSemaphore.WaitAsync(cancellationToken);

            try
            {
                var connection = new SqlConnection(_connectionString);

                // ลอง connect พร้อม health check
                await _circuitBreaker.ExecuteAsync(async () =>
                {
                    await connection.OpenAsync(cancellationToken);
                    await HealthCheckAsync(connection, cancellationToken);
                });

                
                return connection;
            }
            catch (Exception ex)
            {
                _connectionSemaphore.Release();
                _logger?.LogError("❌ Failed to get connection", ex);
                throw;
            }
        }

        /// <summary>
        /// คืน Connection กลับ Pool
        /// </summary>
        public void ReleaseConnection(SqlConnection connection)
        {
            try
            {
                if (connection != null)
                {
                    if (connection.State != System.Data.ConnectionState.Closed)
                    {
                        connection.Close();
                    }
                    connection.Dispose();
                   
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("⚠️ Error releasing connection", ex);
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }

        /// <summary>
        /// ตรวจสอบสุขภาพของ Connection
        /// </summary>
        private async Task HealthCheckAsync(SqlConnection connection, CancellationToken cancellationToken = default)
        {
            using (var command = new SqlCommand("SELECT 1", connection))
            {
                command.CommandTimeout = 5; // Timeout สั้นสำหรับ health check
                await command.ExecuteScalarAsync(cancellationToken);
            }
        }

        /// <summary>
        /// ตรวจสอบสุขภาพของ Connection Pool
        /// </summary>
        public async Task<(bool isHealthy, string message)> CheckPoolHealthAsync()
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    await HealthCheckAsync(connection);
                    return (true, $"✅ Pool healthy - Circuit: {_circuitBreaker.State}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"❌ Pool unhealthy: {ex.Message}");
            }
        }

        /// <summary>
        /// รีเซ็ต Circuit Breaker (ใช้เมื่อต้องการบังคับให้ลองใหม่)
        /// </summary>
        public void ResetCircuitBreaker()
        {
            _circuitBreaker.Reset();
            _logger?.LogInfo("🔄 Circuit Breaker reset");
        }

        /// <summary>
        /// ดึงสถานะปัจจุบันของ Circuit Breaker
        /// </summary>
        public CircuitBreakerState GetCircuitState() => _circuitBreaker.State;

        public void Dispose()
        {
            if (!_disposed)
            {
                _connectionSemaphore?.Dispose();
                _disposed = true;
                _logger?.LogInfo("🔌 Connection Pool disposed");
            }
        }
    }

    /// <summary>
    /// Circuit Breaker Pattern Implementation
    /// </summary>
    public class CircuitBreaker
    {
        private readonly int _failureThreshold;
        private readonly int _successThreshold;
        private readonly TimeSpan _timeout;
        private readonly LogManager _logger;

        private int _failureCount = 0;
        private int _successCount = 0;
        private CircuitBreakerState _state = CircuitBreakerState.Closed;
        private DateTime _lastFailureTime = DateTime.MinValue;
        private readonly object _lock = new object();

        public CircuitBreakerState State
        {
            get
            {
                lock (_lock)
                {
                    // ตรวจสอบว่าถึงเวลา retry หรือยัง (Half-Open)
                    if (_state == CircuitBreakerState.Open &&
                        DateTime.UtcNow - _lastFailureTime >= _timeout)
                    {
                        _state = CircuitBreakerState.HalfOpen;
                        _successCount = 0;
                        _logger?.LogInfo("🔄 Circuit -> HALF-OPEN (attempting recovery)");
                    }
                    return _state;
                }
            }
        }

        public CircuitBreaker(int failureThreshold, int successThreshold, TimeSpan timeout, LogManager logger)
        {
            _failureThreshold = failureThreshold;
            _successThreshold = successThreshold;
            _timeout = timeout;
            _logger = logger;
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            if (State == CircuitBreakerState.Open)
            {
                throw new InvalidOperationException("Circuit breaker is OPEN");
            }

            try
            {
                var result = await action();
                OnSuccess();
                return result;
            }
            catch (Exception ex)
            {
                OnFailure();
                throw;
            }
        }

        public async Task ExecuteAsync(Func<Task> action)
        {
            if (State == CircuitBreakerState.Open)
            {
                throw new InvalidOperationException("Circuit breaker is OPEN");
            }

            try
            {
                await action();
                OnSuccess();
            }
            catch (Exception ex)
            {
                OnFailure();
                throw;
            }
        }

        private void OnSuccess()
        {
            lock (_lock)
            {
                _failureCount = 0;

                if (_state == CircuitBreakerState.HalfOpen)
                {
                    _successCount++;

                    if (_successCount >= _successThreshold)
                    {
                        _state = CircuitBreakerState.Closed;
                        _successCount = 0;
                        _logger?.LogInfo("✅ Circuit -> CLOSED (recovered)");
                    }
                }
            }
        }

        private void OnFailure()
        {
            lock (_lock)
            {
                _failureCount++;
                _lastFailureTime = DateTime.UtcNow;

                if (_state == CircuitBreakerState.HalfOpen)
                {
                    // ถ้าล้มเหลวตอน Half-Open ให้กลับไปเป็น Open ทันที
                    _state = CircuitBreakerState.Open;
                    _failureCount = 0;
                    _successCount = 0;
                    _logger?.LogWarning($"⚠️ Circuit -> OPEN (failed during recovery)");
                }
                else if (_failureCount >= _failureThreshold)
                {
                    _state = CircuitBreakerState.Open;
                    _logger?.LogWarning($"⚠️ Circuit -> OPEN (threshold reached: {_failureCount})");
                }
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _state = CircuitBreakerState.Closed;
                _failureCount = 0;
                _successCount = 0;
                _lastFailureTime = DateTime.MinValue;
            }
        }
    }

    public enum CircuitBreakerState
    {
        Closed,    // ปกติ ทำงานได้
        Open,      // มีปัญหา ปฏิเสธ request ทั้งหมด
        HalfOpen   // กำลังทดสอบว่า recover แล้วหรือยัง
    }
}