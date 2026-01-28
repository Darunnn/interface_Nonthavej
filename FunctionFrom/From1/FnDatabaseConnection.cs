using interface_Nonthavej.Utils;
using Microsoft.Data.SqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace interface_Nonthavej.FunctionFrom.From1
{
    public class FnDatabaseConnection
    {
        private readonly LogManager _logger;
        private readonly string _connectionString;
        private bool _isDatabaseConnected = false;
        private DateTime _lastConnectedTime = DateTime.MinValue;

        public bool IsDatabaseConnected => _isDatabaseConnected;
        public DateTime LastConnectedTime => _lastConnectedTime;

        public FnDatabaseConnection(string connectionString, LogManager logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        /// <summary>
        /// Check database connection status
        /// </summary>
        public (bool isConnected, DateTime? disconnectTime) CheckConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger?.LogWarning("Connection string is null or empty");
                return (false, null);
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    if (!_isDatabaseConnected)
                    {
                        _isDatabaseConnected = true;
                        _lastConnectedTime = DateTime.Now;
                        _logger?.LogInfo("✅ Database connected successfully");
                        _logger.LogConnectDatabase(true, DateTime.Now);
                    }

                    connection.Close();
                    return (true, null);
                }
                catch (SqlException sqlEx)
                {
                    _logger?.LogWarning($"❌ Database connection failed: {sqlEx.Message}");

                    if (_isDatabaseConnected)
                    {
                        _isDatabaseConnected = false;
                        DateTime disconnectTime = DateTime.Now;
                        _logger.LogConnectDatabase(false, _lastConnectedTime, disconnectTime);
                        return (false, disconnectTime);
                    }

                    return (false, DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning($"❌ Unexpected error: {ex.Message}");

                    if (_isDatabaseConnected)
                    {
                        _isDatabaseConnected = false;
                        return (false, DateTime.Now);
                    }

                    return (false, null);
                }
            }
        }

        /// <summary>
        /// Update UI for connected state
        /// </summary>
        public void UpdateConnectedUI(
            Label connectionStatusLabel,
            Button startStopButton,
            bool isServiceRunning,
            Action<string> updateStatusAction)
        {
            try
            {
                connectionStatusLabel.Text = $"Database: 🟢 Connected (Last Connected: {_lastConnectedTime:yyyy-MM-dd HH:mm:ss})";
                connectionStatusLabel.ForeColor = Color.Green;
                startStopButton.Enabled = true;
                startStopButton.BackColor = Color.FromArgb(52, 152, 219);

                if (!isServiceRunning)
                {
                    updateStatusAction?.Invoke("⏹ Stopped - Ready to start");
                }

                _logger?.LogInfo($"UI updated - database connected at {_lastConnectedTime:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error updating connected UI", ex);
            }
        }

        /// <summary>
        /// Update UI for disconnected state
        /// </summary>
        public void UpdateDisconnectedUI(
            Label connectionStatusLabel,
            Button startStopButton,
            DateTime disconnectTime,
            Action<string> updateStatusAction)
        {
            try
            {
                string lastConnectInfo = _lastConnectedTime != DateTime.MinValue
                    ? $" (Last Connected: {_lastConnectedTime:yyyy-MM-dd HH:mm:ss})"
                    : "";

                connectionStatusLabel.Text = $"Database: 🔴 Disconnected (Disconnected at: {disconnectTime:yyyy-MM-dd HH:mm:ss}){lastConnectInfo}";
                connectionStatusLabel.ForeColor = Color.Red;
                startStopButton.Enabled = false;
                startStopButton.BackColor = Color.Gray;

                updateStatusAction?.Invoke("🔴 Database Disconnected - Service stopped");

                _logger?.LogInfo($"UI updated - database disconnected at {disconnectTime:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error updating disconnected UI", ex);
            }
        }
    }
}
