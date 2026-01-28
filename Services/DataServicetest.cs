using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Data;
using interface_Nonthavej.Models;
using interface_Nonthavej.Utils;

namespace interface_Nonthavej.Services
{
    public class DataServicetest
    {
        private readonly string _connectionString;
        private readonly LogManager _logger;
        private readonly int _batchSize;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            WriteIndented = true, // เปลี่ยนเป็น true เพื่อให้อ่านง่าย
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
        };

        public DataServicetest(string connectionString, LogManager logger = null, int batchSize = 100)
        {
            _connectionString = connectionString;
            _logger = logger ?? new LogManager();
            _batchSize = batchSize;
        }

        private string ToNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private PrescriptionBodyRequest BuildPrescriptionBody(IDataReader reader)
        {
            try
            {
                var seq = reader["f_seq"]?.ToString();
                var prescriptionDate = reader["f_prescriptionnodate"]?.ToString();
                var prescriptionDateFormatted = ExtractDate(prescriptionDate);
                var sex = ProcessSex(reader["f_sex"]?.ToString());
                var prnValue = reader["f_PRN"]?.ToString();
                var prn = ProcessPRN(prnValue, 1);
                var stat = ProcessPRN(prnValue, 2);

                return new PrescriptionBodyRequest
                {
                    UniqID = $"{ToNull(reader["f_prescriptionno"]?.ToString())}{ToNull(reader["f_seq"]?.ToString())}",
                    f_prescriptionno = ToNull(reader["f_prescriptionno"]?.ToString()),
                    f_seq = decimal.TryParse(seq, out decimal seqVal) ? seqVal : (decimal?)null,
                    f_seqmax = decimal.TryParse(reader["f_seqmax"]?.ToString(), out decimal seqmax) ? seqmax : (decimal?)null,
                    f_prescriptiondate = ToNull(prescriptionDateFormatted),
                    f_ordercreatedate = ToNull(reader["f_orderacceptdate"]?.ToString()),
                    f_ordertargetdate = ToNull(reader["f_ordertargetdate"]?.ToString()),
                    f_ordertargettime = DateTime.TryParse(reader["f_ordertargettime"]?.ToString(), out var dt)
                                        ? dt.ToString("HH:mm")
                                        : null,
                    f_doctorcode = ToNull(reader["f_doctorcode"]?.ToString()),
                    f_doctorname = ToNull(reader["f_doctorname"]?.ToString()),
                    f_useracceptby = ToNull(reader["f_useracceptby"]?.ToString()),
                    f_orderacceptdate = ToNull(reader["f_orderacceptdate"]?.ToString()),
                    f_orderacceptfromip = ToNull(reader["f_orderacceptfromip"]?.ToString()),
                    f_pharmacylocationcode = ToNull(reader["f_pharmacylocationcode"]?.ToString()),
                    f_pharmacylocationdesc = ToNull(reader["f_pharmacylocationdesc"]?.ToString()),
                    f_prioritycode = ToNull(reader["f_prioritycode"]?.ToString()),
                    f_prioritydesc = ToNull(reader["f_prioritydesc"]?.ToString()),
                    f_hn = ToNull(reader["f_hn"]?.ToString()),
                    f_an = ToNull(reader["f_en"]?.ToString()),
                    f_vn = null,
                    f_title = null,
                    f_patientname = ToNull(reader["f_patientname"]?.ToString()),
                    f_sex = ToNull(sex),
                    f_patientdob = ToNull(reader["f_patientdob"]?.ToString()),
                    f_wardcode = ToNull(reader["f_wardcode"]?.ToString()),
                    f_warddesc = ToNull(reader["f_warddesc"]?.ToString()),
                    f_roomcode = ToNull(reader["f_roomcode"]?.ToString()),
                    f_roomdesc = ToNull(reader["f_roomdesc"]?.ToString()),
                    f_bedcode = ToNull(reader["f_bedcode"]?.ToString()),
                    f_beddesc = ToNull(reader["f_beddesc"]?.ToString()),
                    f_right = null,
                    f_drugallergy = null,
                    f_diagnosis = null,
                    f_orderitemcode = ToNull(reader["f_orderitemcode"]?.ToString()),
                    f_orderitemname = ToNull(reader["f_orderitemname"]?.ToString()),
                    f_orderitemnameTH = ToNull(reader["f_Thai_Name"]?.ToString()),
                    f_orderitemnamegeneric = ToNull(reader["f_orderitemgenericname"]?.ToString()),
                    f_orderqty = decimal.TryParse(reader["f_orderqty"]?.ToString(), out decimal qty) ? qty : (decimal?)null,
                    f_orderunitcode = ToNull(reader["f_orderunitcode"]?.ToString()),
                    f_orderunitdesc = ToNull(reader["f_orderunitdesc"]?.ToString()),
                    f_dosage = decimal.TryParse(reader["f_dosage"]?.ToString(), out decimal dosage) ? dosage : (decimal?)null,
                    f_dosageunit = ToNull(reader["f_dosageunit"]?.ToString()),
                    f_dosagetext = null,
                    f_drugformcode = ToNull(reader["f_drugform"]?.ToString()),
                    f_drugformdesc = ToNull(reader["f_drugformname"]?.ToString()),
                    f_HAD = ToNull(reader["f_heighAlertDrug"]?.ToString()) ?? "0",
                    f_narcoticFlg = ToNull(reader["f_narcoticDrug"]?.ToString()) ?? "0",
                    f_psychotropic = ToNull(reader["f_psychotropicDrug"]?.ToString()) ?? "0",
                    f_binlocation = null,
                    f_itemidentify = ToNull(reader["f_itemidentify"]?.ToString()),
                    f_itemlotno = ToNull(reader["f_itemlotcode"]?.ToString()),
                    f_itemlotexpire = ToNull(reader["f_itemlotexpire"]?.ToString()),
                    f_instructioncode = ToNull(reader["f_instructioncode"]?.ToString()),
                    f_instructiondesc = ToNull(reader["f_instructiondesc"]?.ToString()),
                    f_frequencycode = ToNull(reader["f_frequencycode"]?.ToString()),
                    f_frequencydesc = ToNull(reader["f_frequencydesc"]?.ToString()),
                    f_timecode = null,
                    f_timedesc = null,
                    f_frequencytime = ToNull(reader["f_frequencyTime"]?.ToString()),
                    f_dosagedispense = ToNull(reader["f_dosagedispense"]?.ToString()),
                    f_dayofweek = null,
                    f_noteprocessing = ToNull(reader["f_aux_label_memo"]?.ToString()),
                    f_prn = ToNull(prn),
                    f_stat = ToNull(stat),
                    f_comment = ToNull(reader["f_comment"]?.ToString()),
                    f_tomachineno = ToNull(reader["f_tomachineno"]?.ToString()),
                    f_ipd_order_recordno = null,
                    f_status = ToNull(reader["f_status"]?.ToString()),
                    f_remark = null,
                    f_labeltext = ToNull(reader["f_aux_local_memo"]?.ToString()),
                    f_ipdpt_record_no = ToNull(reader["f_ipdpt_record_no"]?.ToString()),
                    f_qr_code = ToNull(reader["f_QR_Code"]?.ToString()),
                    f_barcode_ref = ToNull(reader["f_BarCodeRef"]?.ToString())
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error building prescription body", ex);
                throw;
            }
        }

        public async Task<(int success, int failed, List<string> errors, string jsonFilePath)> ProcessAndExportToJsonAsync()
        {
            int successCount = 0;
            int failedCount = 0;
            var errors = new List<string>();
            var allPrescriptions = new List<PrescriptionBodyRequest>();
            var currentDate = DateHelper.GetCurrentDateChristianEra();

            string query = $@"
                SELECT TOP (10)
                    [f_prescriptionno],[f_seq],[f_seqmax],[f_prescriptionnodate],
                    [f_prioritycode],[f_prioritydesc],[f_durationcode],[f_durationdesc],
                    [f_orderitemcode],[f_orderitemname],[f_Thai_Name],[f_orderitemgenericname],
                    [f_orderqty],[f_orderunitcode],[f_orderunitdesc],[f_drugform],
                    [f_dosage],[f_dosagedispense],[f_dosageunit],[f_drugformname],
                    [f_ordercreatedate],[f_ordercreatetime],[f_orderacceptdate],
                    [f_orderaccepttime],[f_orderacceptfromip],[f_ordertargetdate],
                    [f_ordertargettime],[f_itemlotcode],[f_itemlotexpire],[f_itemidentify],
                    [f_noteprocessing],[f_ipdpt_record_no],[f_narcoticDrug],
                    [f_psychotropicDrug],[f_heighAlertDrug],[f_useracceptby],
                    [f_userorderby],[f_instructioncode],[f_instructiondesc],
                    [f_frequencycode],[f_frequencydesc],[f_frequencyTime],
                    [f_sex],[f_PRN],[f_patientname],[f_patientepisodedate],
                    [f_en],[f_patientdob],[f_hn],[f_daily_dose_flag],
                    [f_doctorcode],[f_doctorname],[f_wardcode],[f_warddesc],
                    [f_roomcode],[f_roomdesc],[f_fromlocationname],
                    [f_pharmacylocationcode],[f_pharmacylocationdesc],
                    [f_tomachineno],[f_freetext1],[f_freetext2],[f_QR_Code],
                    [f_freetext4],[f_bedcode],[f_beddesc],[f_referenceCode],
                    [f_status],[f_num_type_desc],[f_num_type_of_daily_dose],
                    [f_dispensestatus],[f_opd_adminby],[f_comment],
                    [f_ipd_admincontinue],[f_opd_admindatetime],[f_opd_adminlocation],
                    [f_bagspecialdrug],[f_opd_adminremark],[f_opd_adminstatus],
                    [f_lastmodified],[f_dosevalue],[f_language],[f_age],
                    [f_ordertype],[f_aux_label_memo],[f_aux_local_memo],[f_BarCodeRef]
                FROM tb_thaneshosp_middle
                WHERE f_dispensestatus='0' 
                    AND CONVERT(varchar(10), f_prescriptionnodate, 112) = @CurrentDate
                ORDER BY f_orderacceptdate";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    _logger?.LogInfo($"📊 เริ่มดึงข้อมูล วันที่: {currentDate}");

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CurrentDate", currentDate);
                        command.Parameters.AddWithValue("@BatchSize", _batchSize);
                        command.CommandTimeout = 60;
                        command.CommandType = CommandType.Text;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                try
                                {
                                    var prescriptionNo = reader["f_prescriptionno"]?.ToString();

                                    if (string.IsNullOrEmpty(prescriptionNo))
                                    {
                                        failedCount++;
                                        continue;
                                    }

                                    var prescriptionBody = BuildPrescriptionBody(reader);
                                    allPrescriptions.Add(prescriptionBody);
                                    successCount++;
                                }
                                catch (Exception ex)
                                {
                                    failedCount++;
                                    _logger?.LogError($"❌ Row Error", ex);
                                }
                            }
                        }
                    }
                }

                // สร้างไฟล์ JSON
                string jsonFilePath = await SaveToJsonFile(allPrescriptions, currentDate);
                _logger?.LogInfo($"✅ บันทึกไฟล์ JSON สำเร็จ: {jsonFilePath}");
                _logger?.LogInfo($"📊 สรุป - Success: {successCount}, Failed: {failedCount}");

                return (successCount, failedCount, errors, jsonFilePath);
            }
            catch (SqlException ex)
            {
                _logger?.LogError("❌ SQL Server Error", ex);
                errors.Add($"Database error: {ex.Message}");
                return (successCount, failedCount, errors, null);
            }
            catch (Exception ex)
            {
                _logger?.LogError("❌ Critical Error", ex);
                errors.Add($"Critical error: {ex.Message}");
                return (successCount, failedCount, errors, null);
            }
        }

        private async Task<string> SaveToJsonFile(List<PrescriptionBodyRequest> prescriptions, string currentDate)
        {
            try
            {
                // สร้างโฟลเดอร์ Export ถ้ายังไม่มี
                string exportFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export");
                if (!Directory.Exists(exportFolder))
                {
                    Directory.CreateDirectory(exportFolder);
                }

                // สร้างชื่อไฟล์ตามวันที่และเวลา
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"Prescription_{currentDate}_{timestamp}.json";
                string filePath = Path.Combine(exportFolder, fileName);

                // แปลงเป็น JSON และบันทึกไฟล์
                string jsonContent = JsonSerializer.Serialize(prescriptions, _jsonOptions);
                await File.WriteAllTextAsync(filePath, jsonContent, Encoding.UTF8);

                return filePath;
            }
            catch (Exception ex)
            {
                _logger?.LogError("❌ Error saving JSON file", ex);
                throw;
            }
        }

        private string ExtractDate(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr))
                return "";

            if (DateTime.TryParse(dateStr, out DateTime date))
                return date.ToString("yyyyMMdd");

            return "";
        }

        private string ProcessSex(string sex)
        {
            if (string.IsNullOrEmpty(sex))
                return "U";

            switch (sex)
            {
                case "1":
                    return "F";
                case "2":
                    return "M";
                default:
                    return "U";
            }
        }

        private string ProcessPRN(string prnValue, int type)
        {
            if (string.IsNullOrEmpty(prnValue) || !int.TryParse(prnValue, out int value))
                return "0";

            return (type == 1 && value == 1) || (type == 2 && value == 2) ? "1" : "0";
        }
    }
}