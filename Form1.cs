using interface_Nonthavej.Configuration;
using interface_Nonthavej.FunctionFrom.From1;
using interface_Nonthavej.Models;
using interface_Nonthavej.Services;
using interface_Nonthavej.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace interface_Nonthavej
{
    public partial class Form1 : Form
    {
        // Core services
        private AppConfig _appConfig;
        private LogManager _logger;
        private DataService _dataService;

        // Helper classes
        private Fnupdatefrom1 _uiHelper;
        private FnExport _exportHelper;
        private FnDatabaseConnection _dbConnectionHelper;

        // Service state
        private bool _isServiceRunning = false;
        private bool _wasServiceRunningBeforeDisconnect = false;
        private CancellationTokenSource _cancellationTokenSource;

        // Timers
        private System.Windows.Forms.Timer _connectionCheckTimer;

        // Data management
        private DataTable _processedDataTable;
        private DataView _filteredDataView;
        private string _currentStatusFilter = "All";

        public Form1()
        {
            InitializeComponent();
            InitializeApplication();
        }

        #region Initialization

        private void InitializeApplication()
        {
            try
            {
                _logger = new LogManager();
                _logger.LogInfo("=== Application Starting ===");

                _appConfig = new AppConfig();
                if (!_appConfig.LoadConfiguration())
                {
                    _logger.LogError("Failed to load configuration");
                    MessageBox.Show("ล้มเหลวในการโหลดการกำหนดค่า", "ข้อผิดพลาด");
                    return;
                }

                if (_appConfig != null && !string.IsNullOrEmpty(_appConfig.ConnectionString))
                {
                    _dataService = new DataService(_appConfig.ConnectionString, _appConfig.ApiEndpoint, _logger);
                    _logger.LogInfo($"DataService initialized");
                }
                else
                {
                    _logger.LogWarning("Connection string is empty or null");
                    MessageBox.Show("Connection string is empty", "ข้อผิดพลาด");
                    return;
                }

                if (_appConfig != null)
                {
                    _logger.LogInfo(_appConfig.GetConfigurationSummary());
                }

                // Initialize helper classes
                _uiHelper = new Fnupdatefrom1(_logger, this);
                _exportHelper = new FnExport(_dataService, _logger, _uiHelper);
                _dbConnectionHelper = new FnDatabaseConnection(_appConfig.ConnectionString, _logger);

                InitializeDataTable();
                _uiHelper.UpdateUIState(startStopButton, statusLabel, _dbConnectionHelper.IsDatabaseConnected, _isServiceRunning);

                // Start connection check timer
                _connectionCheckTimer = new System.Windows.Forms.Timer();
                _connectionCheckTimer.Interval = 3000;
                _connectionCheckTimer.Tick += ConnectionCheckTimer_Tick;
                _connectionCheckTimer.Start();

                _logger.LogInfo("Connection check timer started");

                Task.Delay(500).ContinueWith(_ => CheckDatabaseConnection());
                _ = LoadInitialDataAsync();

                _logger.LogInfo("Application initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error initializing application", ex);
                MessageBox.Show($"ข้อผิดพลาดการเริ่มต้น: {ex.Message}", "ข้อผิดพลาด");
            }
        }

        private void InitializeDataTable()
        {
            try
            {
                _processedDataTable = new DataTable();
                _processedDataTable.Columns.Add("Transaction DateTime", typeof(string));
                _processedDataTable.Columns.Add("Order No", typeof(string));
                _processedDataTable.Columns.Add("HN", typeof(string));
                _processedDataTable.Columns.Add("Patient Name", typeof(string));
                _processedDataTable.Columns.Add("Status", typeof(string));

                _filteredDataView = new DataView(_processedDataTable);

                if (dataGridView != null)
                {
                    dataGridView.DataSource = _filteredDataView;

                    dataGridView.Columns["Transaction DateTime"].Width = 165;
                    dataGridView.Columns["Order No"].Width = 120;
                    dataGridView.Columns["HN"].Width = 90;
                    dataGridView.Columns["Patient Name"].Width = 180;
                    dataGridView.Columns["Status"].Width = 100;

                    dataGridView.CellFormatting += DataGridView_CellFormatting;
                }

                InitializePanelFilters();
                _logger?.LogInfo("DataTable initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error initializing DataTable", ex);
            }
        }

        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            _uiHelper.ApplyCellFormatting(dataGridView, e);
        }

        private void InitializePanelFilters()
        {
            try
            {
                // Attach click events to panels
                totalPanel.Click += TotalPanel_Click;
                successPanel.Click += SuccessPanel_Click;
                failedPanel.Click += FailedPanel_Click;

                // Set cursor for all labels inside panels
                foreach (Control ctrl in totalPanel.Controls)
                {
                    if (ctrl is Label)
                    {
                        ctrl.Click += TotalPanel_Click;
                        ctrl.Cursor = Cursors.Hand;
                    }
                }
                foreach (Control ctrl in successPanel.Controls)
                {
                    if (ctrl is Label)
                    {
                        ctrl.Click += SuccessPanel_Click;
                        ctrl.Cursor = Cursors.Hand;
                    }
                }
                foreach (Control ctrl in failedPanel.Controls)
                {
                    if (ctrl is Label)
                    {
                        ctrl.Click += FailedPanel_Click;
                        ctrl.Cursor = Cursors.Hand;
                    }
                }

                totalPanel.Cursor = Cursors.Hand;
                successPanel.Cursor = Cursors.Hand;
                failedPanel.Cursor = Cursors.Hand;

                _logger?.LogInfo("Panel filters initialized");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error initializing panel filters", ex);
            }
        }

        #endregion

        #region Filter Management

        private void TotalPanel_Click(object sender, EventArgs e)
        {
            _currentStatusFilter = "All";
            ApplyStatusFilter();
        }

        private void SuccessPanel_Click(object sender, EventArgs e)
        {
            _currentStatusFilter = "Success";
            ApplyStatusFilter();
        }

        private void FailedPanel_Click(object sender, EventArgs e)
        {
            _currentStatusFilter = "Failed";
            ApplyStatusFilter();
        }

        private void ApplyStatusFilter()
        {
            try
            {
                if (_filteredDataView == null) return;

                if (_currentStatusFilter == "All")
                {
                    _filteredDataView.RowFilter = string.Empty;
                }
                else
                {
                    _filteredDataView.RowFilter = $"[Status] = '{_currentStatusFilter}'";
                }

                _uiHelper.UpdateStatusFilterUI(totalPanel, successPanel, failedPanel, _currentStatusFilter);
                _uiHelper.UpdateSummaryCounts(_processedDataTable, totalCountLabel, successCountLabel, failedCountLabel);

                _logger?.LogInfo($"Filter applied: {_currentStatusFilter}");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error applying status filter", ex);
            }
        }

        #endregion

        #region Data Loading

        private async Task LoadInitialDataAsync()
        {
            try
            {
                _logger?.LogInfo("⏳ Loading initial data...");
                await Task.Delay(500);
                string currentDate = DateHelper.ConvertToChristianEraFormatted(DateTime.Now);
                await LoadDataGridViewAsync(currentDate, "");
                _logger?.LogInfo("✅ Initial data loaded successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error loading initial data", ex);
            }
        }

        private async Task LoadDataGridViewAsync(string date = "", string searchText = "")
        {
            try
            {
                string queryDate = string.IsNullOrEmpty(date)
                    ? DateHelper.GetCurrentDateChristianEra()
                    : date.Replace("-", "");

                _logger?.LogInfo($"🔍 [DEBUG] Loading grid data - Input date: '{date}', Query date: '{queryDate}', Search: '{searchText}'");

                if (_dataService == null)
                {
                    _logger?.LogWarning("⚠️ DataService is not initialized");
                    return;
                }

                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        if (!_isServiceRunning)
                        {
                            _uiHelper.UpdateStatus(statusLabel, "⏳ Loading data...");
                        }
                    });
                }
                else
                {
                    if (!_isServiceRunning)
                    {
                        _uiHelper.UpdateStatus(statusLabel, "⏳ Loading data...");
                    }
                }

                var data = await _dataService.GetPrescriptionDataAsync(queryDate, searchText);

                _logger?.LogInfo($"📊 [DEBUG] Retrieved {data.Count} records from database");

                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        _uiHelper.UpdateGridView(_processedDataTable, data, _isServiceRunning,
                            (status) => _uiHelper.UpdateStatus(statusLabel, status));
                        _uiHelper.UpdateSummaryCounts(_processedDataTable, totalCountLabel, successCountLabel, failedCountLabel);

                        if (dataGridView.DataSource == null)
                        {
                            dataGridView.DataSource = _filteredDataView;
                        }
                        else
                        {
                            dataGridView.Refresh();
                        }
                    });
                }
                else
                {
                    _uiHelper.UpdateGridView(_processedDataTable, data, _isServiceRunning,
                        (status) => _uiHelper.UpdateStatus(statusLabel, status));
                    _uiHelper.UpdateSummaryCounts(_processedDataTable, totalCountLabel, successCountLabel, failedCountLabel);

                    if (dataGridView.DataSource == null)
                    {
                        dataGridView.DataSource = _filteredDataView;
                    }
                    else
                    {
                        dataGridView.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("❌ Error loading DataGridView", ex);

                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        _uiHelper.UpdateStatus(statusLabel, "❌ Error loading data");
                        _uiHelper.ShowAutoClosingMessageBox(this, $"ข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด");
                    });
                }
                else
                {
                    _uiHelper.UpdateStatus(statusLabel, "❌ Error loading data");
                    _uiHelper.ShowAutoClosingMessageBox(this, $"ข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด");
                }
            }
        }

        #endregion

        #region Database Connection Management

        private void ConnectionCheckTimer_Tick(object sender, EventArgs e)
        {
            CheckDatabaseConnection();
        }

        private void CheckDatabaseConnection()
        {
            var (isConnected, disconnectTime) = _dbConnectionHelper.CheckConnection();

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (isConnected)
                    {
                        _dbConnectionHelper.UpdateConnectedUI(
                            connectionStatusLabel,
                            startStopButton,
                            _isServiceRunning,
                            (status) => _uiHelper.UpdateStatus(statusLabel, status)
                        );

                        if (_wasServiceRunningBeforeDisconnect)
                        {
                            _logger?.LogInfo("🔄 Auto-restarting service after database reconnection");
                            StartService();
                            _wasServiceRunningBeforeDisconnect = false;
                        }
                    }
                    else if (disconnectTime.HasValue)
                    {
                        if (_isServiceRunning)
                        {
                            _wasServiceRunningBeforeDisconnect = true;
                            _logger?.LogInfo("⚠️ Service was running before disconnect - will auto-restart when reconnected");
                            StopService();
                        }
                        else
                        {
                            _wasServiceRunningBeforeDisconnect = false;
                        }

                        _dbConnectionHelper.UpdateDisconnectedUI(
                            connectionStatusLabel,
                            startStopButton,
                            disconnectTime.Value,
                            (status) => _uiHelper.UpdateStatus(statusLabel, status)
                        );
                    }
                });
            }
            else
            {
                if (isConnected)
                {
                    _dbConnectionHelper.UpdateConnectedUI(
                        connectionStatusLabel,
                        startStopButton,
                        _isServiceRunning,
                        (status) => _uiHelper.UpdateStatus(statusLabel, status)
                    );

                    if (_wasServiceRunningBeforeDisconnect)
                    {
                        _logger?.LogInfo("🔄 Auto-restarting service after database reconnection");
                        StartService();
                        _wasServiceRunningBeforeDisconnect = false;
                    }
                }
                else if (disconnectTime.HasValue)
                {
                    if (_isServiceRunning)
                    {
                        _wasServiceRunningBeforeDisconnect = true;
                        _logger?.LogInfo("⚠️ Service was running before disconnect - will auto-restart when reconnected");
                        StopService();
                    }
                    else
                    {
                        _wasServiceRunningBeforeDisconnect = false;
                    }

                    _dbConnectionHelper.UpdateDisconnectedUI(
                        connectionStatusLabel,
                        startStopButton,
                        disconnectTime.Value,
                        (status) => _uiHelper.UpdateStatus(statusLabel, status)
                    );
                }
            }
        }

        #endregion

        #region Service Control

        private void StartStopButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_dbConnectionHelper.IsDatabaseConnected)
                {
                    _logger?.LogWarning("Cannot start - database not connected");
                    _uiHelper.ShowAutoClosingMessageBox(this, "ไม่สามารถเชื่อมต่อฐานข้อมูล", "ข้อผิดพลาด");
                    return;
                }

                if (_isServiceRunning)
                {
                    _logger?.LogInfo("Stopping service");
                    StopService();
                }
                else
                {
                    _logger?.LogInfo("Starting service");
                    StartService();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in StartStopButton_Click", ex);
                _uiHelper.ShowAutoClosingMessageBox(this, $"ข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด");
            }
        }

        private void StartService()
        {
            try
            {
                if (_isServiceRunning) return;
                if (!_dbConnectionHelper.IsDatabaseConnected) return;
                if (_dataService == null) return;

                _isServiceRunning = true;
                _cancellationTokenSource = new CancellationTokenSource();

                this.Invoke((MethodInvoker)delegate
                {
                    _uiHelper.UpdateServiceRunningUI(startStopButton, statusLabel);
                });

                _logger.LogInfo("Service started");
                Task.Run(() => ProcessDataLoop(_cancellationTokenSource.Token));
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error starting service", ex);
                _isServiceRunning = false;
                this.Invoke((MethodInvoker)delegate {
                    _uiHelper.UpdateUIState(startStopButton, statusLabel, _dbConnectionHelper.IsDatabaseConnected, _isServiceRunning);
                });
            }
        }

        private void StopService()
        {
            try
            {
                _isServiceRunning = false;

                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                }

                this.Invoke((MethodInvoker)delegate
                {
                    _uiHelper.UpdateServiceStoppedUI(startStopButton, statusLabel);
                });

                _logger.LogInfo("Service stopped");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error stopping service", ex);
            }
        }

        private async Task ProcessDataLoop(CancellationToken cancellationToken)
        {
            int loopCount = 0;

            while (!cancellationToken.IsCancellationRequested && _isServiceRunning)
            {
                try
                {
                    loopCount++;

                    this.Invoke((MethodInvoker)delegate
                    {
                        _uiHelper.UpdateStatus(statusLabel, $"▶ Running - Checking for new data... (Loop #{loopCount})");
                    });

                    var (successCount, failedCount, errors) = await _dataService.ProcessAndSendDataAsync();

                    int totalFound = successCount + failedCount;

                    this.Invoke((MethodInvoker)delegate
                    {
                        _uiHelper.UpdateLastCheckLabels(lastCheckLabel, lastSuccessLabel, lastFoundLabel,
                            successCount, failedCount);

                        if (totalFound > 0)
                        {
                            _uiHelper.UpdateStatus(statusLabel,
                                $"▶ Running - Processed {totalFound} items ({successCount} success, {failedCount} failed)");

                            string currentDate = DateHelper.ConvertToChristianEraFormatted(DateTime.Now);
                            Task.Run(() => LoadDataGridViewAsync(currentDate));
                        }
                        else
                        {
                            _uiHelper.UpdateStatus(statusLabel, "▶ Running - No new data found");
                        }
                    });

                    foreach (var error in errors)
                    {
                        _logger.LogWarning(error);
                    }

                    _logger.LogInfo($"Loop #{loopCount} Complete: {successCount} success, {failedCount} failed");

                    int delaySeconds = _appConfig?.ProcessingIntervalSeconds ?? 15;

                    _logger.LogInfo($"⏳ Waiting {delaySeconds}s before next check...");

                    for (int i = delaySeconds; i > 0; i--)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        this.Invoke((MethodInvoker)delegate
                        {
                            _uiHelper.UpdateStatus(statusLabel, $"▶ Running - Waiting {i}s for next check...");
                        });

                        await Task.Delay(1000, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger?.LogInfo("ProcessDataLoop cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger?.LogError("Error in process loop", ex);

                    this.Invoke((MethodInvoker)delegate
                    {
                        _uiHelper.UpdateStatus(statusLabel, "⚠️ Error - Retrying in 10s...");
                    });

                    await Task.Delay(10000, cancellationToken);
                }
            }

            _logger?.LogInfo("ProcessDataLoop ended");
        }

        #endregion

        #region Button Event Handlers

        private async void SearchButton_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedDate = DateHelper.ConvertToChristianEraFormatted(dateTimePicker.Value);
                string searchText = searchTextBox.Text.Trim();

                _logger?.LogInfo($"🔍 Search initiated - Date: {selectedDate}, Search: '{searchText}'");

                _uiHelper.UpdateStatus(statusLabel, $"🔍 Searching for '{searchText}' on {selectedDate}...");

                await LoadDataGridViewAsync(selectedDate, searchText);

                _uiHelper.UpdateStatus(statusLabel, "✅ Search completed");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in SearchButton_Click", ex);
                _uiHelper.UpdateStatus(statusLabel, "❌ Search failed");
                _uiHelper.ShowAutoClosingMessageBox(this, $"ข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด");
            }
        }

        private async void RefreshButton_Click(object sender, EventArgs e)
        {
            try
            {
                _logger?.LogInfo("Refresh button clicked");

                _currentStatusFilter = "All";
                searchTextBox.Clear();
                dateTimePicker.Value = DateTime.Now;

                _uiHelper.UpdateStatus(statusLabel, "🔄 Refreshing data...");

                string currentDate = DateHelper.ConvertToChristianEraFormatted(DateTime.Now);
                await LoadDataGridViewAsync(currentDate, "");

                _uiHelper.UpdateStatus(statusLabel, "✅ Data refreshed");

                _logger?.LogInfo($"Data refreshed - Reset to current date: {currentDate}");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in RefreshButton_Click", ex);
                _uiHelper.UpdateStatus(statusLabel, "❌ Refresh failed");
                _uiHelper.ShowAutoClosingMessageBox(this, $"ข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด");
            }
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            try
            {
                _logger?.LogInfo("Settings button clicked");

                using (var settingsForm = new SettingsForm())
                {
                    var result = settingsForm.ShowDialog(this);

                    if (result == DialogResult.OK && settingsForm.SettingsChanged)
                    {
                        _logger?.LogInfo("Settings were changed, reloading configuration...");

                        _appConfig = new AppConfig();
                        if (_appConfig.LoadConfiguration())
                        {
                            _dataService = new DataService(_appConfig.ConnectionString, _appConfig.ApiEndpoint, _logger);
                            _dbConnectionHelper = new FnDatabaseConnection(_appConfig.ConnectionString, _logger);
                            _exportHelper = new FnExport(_dataService, _logger, _uiHelper);

                            _logger?.LogInfo("Configuration reloaded successfully");

                            _uiHelper.ShowAutoClosingMessageBox(
                                this,
                                "✅ การตั้งค่าได้รับการอัพเดทแล้ว\nบางการตั้งค่าอาจต้อง Restart โปรแกรม",
                                "สำเร็จ",
                                3000
                            );

                            CheckDatabaseConnection();
                        }
                        else
                        {
                            _logger?.LogError("Failed to reload configuration");
                            _uiHelper.ShowAutoClosingMessageBox(
                                this,
                                "⚠️ ไม่สามารถโหลดการตั้งค่าใหม่ได้\nกรุณาตรวจสอบไฟล์ config",
                                "คำเตือน"
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in SettingsButton_Click", ex);
                _uiHelper.ShowAutoClosingMessageBox(this, $"ข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด");
            }
        }

        private async void ExportButton_Click(object sender, EventArgs e)
        {
            try
            {
                _logger?.LogInfo("Export button clicked");

                if (dataGridView.Rows.Count == 0)
                {
                    _uiHelper.ShowAutoClosingMessageBox(this, "ไม่มีข้อมูลให้ Export", "แจ้งเตือน");
                    return;
                }

                if (dataGridView.SelectedRows.Count == 0)
                {
                    _uiHelper.ShowAutoClosingMessageBox(this, "กรุณาเลือกข้อมูลที่ต้องการ Export ก่อน", "แจ้งเตือน");
                    return;
                }

                await _exportHelper.ExportSelectedRowsAsync(this, dataGridView,
                    (status) => _uiHelper.UpdateStatus(statusLabel, status));
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in ExportButton_Click", ex);
                _uiHelper.ShowAutoClosingMessageBox(this, $"ข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด");
            }
        }

        #endregion

        #region Form Lifecycle

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isServiceRunning)
            {
                var result = MessageBox.Show(
                    "Service กำลังทำงานอยู่ คุณต้องการปิดโปรแกรมใช่หรือไม่?",
                    "ยืนยันการปิด",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            try
            {
                _logger?.LogInfo("=== Application Closing ===");

                if (_isServiceRunning)
                {
                    _logger?.LogInfo("Stopping service");
                    StopService();
                    Thread.Sleep(1000);
                }

                _connectionCheckTimer?.Stop();
                _connectionCheckTimer?.Dispose();
                _cancellationTokenSource?.Dispose();

                _logger?.LogInfo("Application closed successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error during form closing", ex);
            }

            base.OnFormClosing(e);
        }

        #endregion
    }
}