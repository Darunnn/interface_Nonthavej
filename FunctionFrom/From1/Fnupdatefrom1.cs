using interface_Nonthavej.Models;
using interface_Nonthavej.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

 namespace interface_Nonthavej.FunctionFrom.From1
{
    public class Fnupdatefrom1
    {
        private readonly LogManager _logger;
        private readonly Form _parentForm;

        public Fnupdatefrom1(LogManager logger, Form parentForm = null)
        {
            _logger = logger;
            _parentForm = parentForm;
        }

        /// <summary>
        /// Update summary count labels
        /// </summary>
        public void UpdateSummaryCounts(DataTable dataTable, Label totalCountLabel, Label successCountLabel, Label failedCountLabel)
        {
            try
            {
                int totalCount = dataTable.Rows.Count;
                int successCount = 0;
                int failedCount = 0;

                foreach (DataRow row in dataTable.Rows)
                {
                    string status = row["Status"]?.ToString() ?? "";
                    if (status == "Success") successCount++;
                    else if (status == "Failed") failedCount++;
                }

                totalCountLabel.Text = totalCount.ToString();
                successCountLabel.Text = successCount.ToString();
                failedCountLabel.Text = failedCount.ToString();

                _logger?.LogInfo($"Summary: Total={totalCount}, Success={successCount}, Failed={failedCount}");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error updating summary counts", ex);
            }
        }

        /// <summary>
        /// Update status filter UI (border style)
        /// </summary>
        public void UpdateStatusFilterUI(Panel totalPanel, Panel successPanel, Panel failedPanel, string currentFilter)
        {
            try
            {
                totalPanel.BorderStyle = (currentFilter == "All")
                    ? BorderStyle.Fixed3D
                    : BorderStyle.FixedSingle;

                successPanel.BorderStyle = (currentFilter == "Success")
                    ? BorderStyle.Fixed3D
                    : BorderStyle.FixedSingle;

                failedPanel.BorderStyle = (currentFilter == "Failed")
                    ? BorderStyle.Fixed3D
                    : BorderStyle.FixedSingle;

                totalPanel.Invalidate();
                successPanel.Invalidate();
                failedPanel.Invalidate();
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error updating filter UI", ex);
            }
        }

        /// <summary>
        /// Apply cell formatting for DataGridView based on status
        /// </summary>
        public void ApplyCellFormatting(DataGridView dataGridView, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && e.RowIndex < dataGridView.Rows.Count)
                {
                    var row = dataGridView.Rows[e.RowIndex];

                    if (row.Cells["Status"].Value != null)
                    {
                        string status = row.Cells["Status"].Value.ToString();

                        if (status == "Success")
                        {
                            row.DefaultCellStyle.BackColor = Color.LightGreen;
                            row.DefaultCellStyle.SelectionBackColor = Color.Green;
                        }
                        else if (status == "Failed")
                        {
                            row.DefaultCellStyle.BackColor = Color.LightCoral;
                            row.DefaultCellStyle.SelectionBackColor = Color.Red;
                        }
                        else
                        {
                            row.DefaultCellStyle.BackColor = Color.White;
                            row.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in cell formatting", ex);
            }
        }

        /// <summary>
        /// Show auto-closing message box
        /// </summary>
        public void ShowAutoClosingMessageBox(Form parentForm, string message, string title = "แจ้งเตือน", int delayMs = 5000)
        {
            try
            {
                var messageForm = new Form
                {
                    Text = title,
                    StartPosition = FormStartPosition.CenterParent,
                    Width = 400,
                    Height = 150,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    ShowInTaskbar = false,
                    TopMost = true
                };

                var messageLabel = new Label
                {
                    Text = message,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Padding = new Padding(10)
                };

                var okButton = new Button
                {
                    Text = "ตกลง",
                    DialogResult = DialogResult.OK,
                    Dock = DockStyle.Bottom,
                    Height = 40
                };

                messageForm.Controls.Add(messageLabel);
                messageForm.Controls.Add(okButton);
                messageForm.AcceptButton = okButton;

                var autoTimer = new System.Windows.Forms.Timer();
                autoTimer.Interval = delayMs;
                autoTimer.Tick += (s, e) =>
                {
                    autoTimer.Stop();
                    messageForm.Close();
                };
                autoTimer.Start();

                messageForm.ShowDialog(parentForm);
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error showing message box", ex);
            }
        }

        /// <summary>
        /// Update grid view with data
        /// </summary>
        public int UpdateGridView(DataTable dataTable, List<GridViewDataModel> data, bool isServiceRunning, Action<string> updateStatusAction)
        {
            try
            {
                _logger?.LogInfo($"📝 [DEBUG] Clearing DataTable, current rows: {dataTable.Rows.Count}");

                dataTable.Rows.Clear();

                _logger?.LogInfo($"➕ [DEBUG] Adding {data.Count} rows to DataTable");

                int addedCount = 0;
                foreach (var item in data)
                {
                    try
                    {
                        string displayStatus = item.Status == "1" ? "Success" :
                                              (item.Status == "3" ? "Failed" : "Pending");
                        string formattedPrescriptionDate = DateTime.Parse(item.Prescriptiondate).ToString("dd/MM/yyyy HH:mm:ss");

                        dataTable.Rows.Add(
                            formattedPrescriptionDate,
                            item.PrescriptionNo,
                            item.HN,
                            item.PatientName,
                            displayStatus
                        );

                        addedCount++;

                        if (addedCount == 1)
                        {
                            _logger?.LogInfo($"📄 [DEBUG] First row: Rx={item.PrescriptionNo}, HN={item.HN}, Status={item.Status}→{displayStatus}");
                        }
                    }
                    catch (Exception rowEx)
                    {
                        _logger?.LogError($"❌ Error adding row: Rx={item.PrescriptionNo}", rowEx);
                    }
                }

                _logger?.LogInfo($"✅ [DEBUG] Added {addedCount}/{data.Count} rows successfully");

                if (isServiceRunning)
                {
                    updateStatusAction?.Invoke($"▶ Running - Grid updated with {addedCount} records");
                }
                else
                {
                    updateStatusAction?.Invoke($"⏹ Stopped - Showing {addedCount} records");
                }

                _logger?.LogInfo($"✅ Grid loaded with {addedCount} rows, Total rows in table: {dataTable.Rows.Count}");

                return addedCount;
            }
            catch (Exception ex)
            {
                _logger?.LogError("❌ Error in UpdateGridView", ex);
                updateStatusAction?.Invoke("❌ Error updating grid");
                return 0;
            }
        }

        /// <summary>
        /// Update status label with thread-safe invoke
        /// </summary>
        public void UpdateStatus(Label statusLabel, string status)
        {
            if (statusLabel == null) return;

            if (statusLabel.InvokeRequired)
            {
                statusLabel.Invoke(new Action<Label, string>(UpdateStatus), statusLabel, status);
                return;
            }
            statusLabel.Text = $"Status: {status}";
        }

        /// <summary>
        /// Update UI state based on database connection and service running status
        /// </summary>
        public void UpdateUIState(Button startStopButton, Label statusLabel, bool isDatabaseConnected, bool isServiceRunning)
        {
            try
            {
                startStopButton.Enabled = isDatabaseConnected;
                startStopButton.BackColor = isDatabaseConnected
                    ? Color.FromArgb(52, 152, 219)
                    : Color.Gray;

                statusLabel.Text = isServiceRunning
                    ? "Status: ▶ Running"
                    : "Status: ⏹ Stopped";

                _logger?.LogInfo($"UI State Updated - DB: {isDatabaseConnected}, Running: {isServiceRunning}");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error updating UI state", ex);
            }
        }

        /// <summary>
        /// Update service button for running state
        /// </summary>
        public void UpdateServiceRunningUI(Button startStopButton, Label statusLabel)
        {
            try
            {
                if (startStopButton.InvokeRequired)
                {
                    startStopButton.Invoke(new Action(() => UpdateServiceRunningUI(startStopButton, statusLabel)));
                    return;
                }

                startStopButton.Text = "⏹ Stop";
                startStopButton.BackColor = Color.FromArgb(231, 76, 60);
                UpdateStatus(statusLabel, "▶ Running - Waiting for data...");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error updating service running UI", ex);
            }
        }

        /// <summary>
        /// Update service button for stopped state
        /// </summary>
        public void UpdateServiceStoppedUI(Button startStopButton, Label statusLabel)
        {
            try
            {
                if (startStopButton.InvokeRequired)
                {
                    startStopButton.Invoke(new Action(() => UpdateServiceStoppedUI(startStopButton, statusLabel)));
                    return;
                }

                startStopButton.Text = "▶ Start";
                startStopButton.BackColor = Color.FromArgb(52, 152, 219);
                UpdateStatus(statusLabel, "⏹ Stopped");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error updating service stopped UI", ex);
            }
        }

        /// <summary>
        /// Update last check labels
        /// </summary>
        public void UpdateLastCheckLabels(Label lastCheckLabel, Label lastSuccessLabel, Label lastFoundLabel,
            int successCount, int failedCount)
        {
            try
            {
                lastCheckLabel.Text = $"Last Check: {DateTime.Now:HH:mm:ss}";

                int totalFound = successCount + failedCount;
                if (totalFound > 0)
                {
                    if (successCount > 0)
                    {
                        lastSuccessLabel.Text = $"Last Success: {DateTime.Now:HH:mm:ss} ({successCount} items)";
                    }
                    lastFoundLabel.Text = $"Last Found: {totalFound} items";
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error updating last check labels", ex);
            }
        }
    }
}


    