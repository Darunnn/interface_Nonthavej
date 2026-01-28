using interface_Nonthavej.Models;
using interface_Nonthavej.Services;
using interface_Nonthavej.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace interface_Nonthavej.FunctionFrom.From1
{
    public class FnExport
    {
        private readonly DataService _dataService;
        private readonly LogManager _logger;
        private readonly Fnupdatefrom1 _Fnupdatefrom1;

        public FnExport(DataService dataService, LogManager logger, Fnupdatefrom1 fnupdatefrom1)
        {
            _dataService = dataService;
            _logger = logger;
            _Fnupdatefrom1 = fnupdatefrom1;
        }

        /// <summary>
        /// Extract prescription list from selected DataGridView rows
        /// </summary>
        public List<(string prescriptionNo, string prescriptionDate)> ExtractPrescriptionListFromGrid(DataGridView dataGridView)
        {
            var prescriptionList = new List<(string prescriptionNo, string prescriptionDate)>();

            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                try
                {
                    string prescriptionNo = row.Cells["Order No"]?.Value?.ToString() ?? "";
                    string transactionDateTime = row.Cells["Transaction DateTime"]?.Value?.ToString() ?? "";

                    string prescriptionDate = DateHelper.ExtractDateInYyyyMmDd(transactionDateTime);

                    if (!string.IsNullOrEmpty(prescriptionNo) && !string.IsNullOrEmpty(prescriptionDate))
                    {
                        prescriptionList.Add((prescriptionNo, prescriptionDate));
                        _logger?.LogInfo($"   Adding: Rx={prescriptionNo}, Date={prescriptionDate}");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning($"Error processing row for export: {ex.Message}");
                }
            }

            return prescriptionList;
        }

        /// <summary>
        /// Generate export filename based on prescription list
        /// </summary>
        public string GenerateExportFilename(List<(string prescriptionNo, string prescriptionDate)> prescriptionList)
        {
            string fileNamePart = "";
            if (prescriptionList.Count == 1)
            {
                fileNamePart = prescriptionList[0].prescriptionNo;
            }
            else if (prescriptionList.Count > 1)
            {
                string firstOrder = prescriptionList[0].prescriptionNo;
                string lastOrder = prescriptionList[prescriptionList.Count - 1].prescriptionNo;
                fileNamePart = $"{firstOrder}_to_{lastOrder}";
            }

            return $"Export_{fileNamePart}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        }

        /// <summary>
        /// Export selected rows to JSON file
        /// </summary>
        public async Task<bool> ExportSelectedRowsAsync(
            Form parentForm,
            DataGridView dataGridView,
            Action<string> updateStatusAction)
        {
            try
            {
                if (dataGridView.SelectedRows.Count == 0)
                {
                    _Fnupdatefrom1.ShowAutoClosingMessageBox(parentForm, "กรุณาเลือกข้อมูลที่ต้องการ Export", "แจ้งเตือน");
                    return false;
                }

                _logger?.LogInfo($"Exporting {dataGridView.SelectedRows.Count} selected rows");

                // Extract prescription list
                var prescriptionList = ExtractPrescriptionListFromGrid(dataGridView);

                if (prescriptionList.Count == 0)
                {
                    _Fnupdatefrom1.ShowAutoClosingMessageBox(parentForm, "ไม่สามารถดึงข้อมูล Prescription ได้", "แจ้งเตือน");
                    return false;
                }

                _logger?.LogInfo($"📦 Fetching full data for {prescriptionList.Count} prescriptions from database...");

                updateStatusAction?.Invoke($"⏳ Loading full data for export ({prescriptionList.Count} prescriptions)...");

                // Get full data from database
                var fullDataList = await _dataService.GetFullPrescriptionDataAsync(prescriptionList);

                if (fullDataList == null || fullDataList.Count == 0)
                {
                    _Fnupdatefrom1.ShowAutoClosingMessageBox(parentForm, "ไม่พบข้อมูลจาก Database", "แจ้งเตือน");
                    updateStatusAction?.Invoke("⚠️ Export cancelled - No data found");
                    return false;
                }

                _logger?.LogInfo($"✅ Retrieved {fullDataList.Count} records from database");

                // Show save file dialog
                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "JSON Files (*.json)|*.json";
                    saveFileDialog.DefaultExt = "json";
                    saveFileDialog.FileName = GenerateExportFilename(prescriptionList);

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveFileDialog.FileName;

                        updateStatusAction?.Invoke($"💾 Exporting to {Path.GetFileName(filePath)}...");

                        // Prepare JSON body
                        var body = new PrescriptionBodyResponse
                        {
                            data = fullDataList.ToArray()
                        };

                        // Serialize to JSON
                        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = null,
                            WriteIndented = true,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
                        });

                        // Write to file
                        await Task.Run(() => File.WriteAllText(filePath, json, Encoding.UTF8));

                        _logger?.LogInfo($"✅ Export completed: {filePath}");
                        _logger?.LogInfo($"   Total records exported: {fullDataList.Count}");
                        _logger?.LogInfo($"   File size: {new FileInfo(filePath).Length / 1024.0:F2} KB");

                        updateStatusAction?.Invoke($"✅ Export completed - {fullDataList.Count} records");

                        _Fnupdatefrom1.ShowAutoClosingMessageBox(
                            parentForm,
                            $"✅ Export สำเร็จ!\n\n" +
                            $"จำนวน Prescriptions: {prescriptionList.Count}\n" +
                            $"จำนวน Records: {fullDataList.Count}\n" +
                            $"ขนาดไฟล์: {new FileInfo(filePath).Length / 1024.0:F2} KB\n" +
                            $"บันทึกที่: {filePath}",
                            "สำเร็จ",
                            3000
                        );

                        return true;
                    }
                    else
                    {
                        updateStatusAction?.Invoke("⚠️ Export cancelled");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error exporting selected rows", ex);
                updateStatusAction?.Invoke("❌ Export failed");
                _Fnupdatefrom1.ShowAutoClosingMessageBox(parentForm, $"ข้อผิดพลาดในการ Export: {ex.Message}", "ข้อผิดพลาด");
                return false;
            }
        }
    }
}
