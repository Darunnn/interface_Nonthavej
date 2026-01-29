using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Data;
using interface_Nonthavej.Models;
using interface_Nonthavej.Utils;

namespace interface_Nonthavej.Services
{
    public class DataService
    {
        private readonly string _connectionString;
        private readonly string _apiUrl;
        private readonly HttpClient _httpClient;
        private readonly LogManager _logger;
        private readonly int _batchSize;


        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
        };

        public DataService(string connectionString, string apiUrl, LogManager logger = null, int batchSize = 100)
        {
            _connectionString = connectionString;
            _apiUrl = apiUrl;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            _httpClient.DefaultRequestHeaders.ConnectionClose = false;
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
                var ordercreateDate = reader["f_orderacceptdate"]?.ToString();
                var ordertargetDate = reader["f_ordertargetdate"]?.ToString();
                var orderacceptDate = reader["f_orderacceptdate"]?.ToString();
                var patientdob = reader["f_patientdob"]?.ToString();
                var itemlotExpire = reader["f_itemlotexpire"]?.ToString();
                var ordercreateDateFormatted = ExtractDate2(ordercreateDate);
                var ordertargetDateFormatted = ExtractDate2(ordertargetDate);
                var orderacceptDateFormatted = ExtractDate2(orderacceptDate);
                var patientdobFormatted = ExtractDate2(patientdob);
                var itemlotExpireFormatted = ExtractDate2(itemlotExpire);
                var sex = ProcessSex(reader["f_sex"]?.ToString());
                var prnValue = reader["f_PRN"]?.ToString();
                var prn = ProcessPRN(prnValue, 1);
                var stat = ProcessPRN(prnValue, 2);

                return new PrescriptionBodyRequest
                {
                    UniqID = $"{ToNull(reader["f_prescriptionno"]?.ToString())}{ToNull(reader["f_seq"]?.ToString())}",
                    f_prescriptionno = ToNull(reader["f_prescriptionno"]?.ToString()),
                    f_seq = decimal.TryParse(seq, out decimal seqVal) ? seqVal : null,
                    f_seqmax = decimal.TryParse(reader["f_seqmax"]?.ToString(), out decimal seqmax) ? seqmax : null,
                    f_prescriptionnodate = ToNull(prescriptionDateFormatted),
                    f_ordercreatedate = ToNull(ordercreateDateFormatted),
                    f_ordertargetdate = ToNull(ordertargetDateFormatted),
                    f_ordertargettime = DateTime.TryParse(reader["f_ordertargettime"]?.ToString(), out var dt)
                                        ? dt.ToString("HH:mm")
                                        : null,
                    f_doctorcode = ToNull(reader["f_doctorcode"]?.ToString()),
                    f_doctorname = ToNull(reader["f_doctorname"]?.ToString()),
                    f_useracceptby = ToNull(reader["f_useracceptby"]?.ToString()),
                    f_orderacceptdate = ToNull(orderacceptDateFormatted),
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
                    f_patientdob = ToNull(patientdobFormatted),
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
                    f_orderqty = decimal.TryParse(reader["f_orderqty"]?.ToString(), out decimal qty) ? qty : null,
                    f_orderunitcode = ToNull(reader["f_orderunitcode"]?.ToString()),
                    f_orderunitdesc = ToNull(reader["f_orderunitdesc"]?.ToString()),
                    f_dosage = decimal.TryParse(reader["f_dosage"]?.ToString(), out decimal dosage) ? dosage : null,
                    f_dosageunit = null,
                    f_dosagetext = null,
                    f_drugformcode = ToNull(reader["f_drugform"]?.ToString()),
                    f_drugformdesc = ToNull(reader["f_drugformname"]?.ToString()),
                    f_HAD = ToNull(reader["f_heighAlertDrug"]?.ToString()) ?? "0",
                    f_narcoticFlg = ToNull(reader["f_narcoticDrug"]?.ToString()) ?? "0",
                    f_psychotropic = ToNull(reader["f_psychotropicDrug"]?.ToString()) ?? "0",
                    f_binlocation = null,
                    f_itemidentify = ToNull(reader["f_itemidentify"]?.ToString()),
                    f_itemlotno = ToNull(reader["f_itemlotcode"]?.ToString()),
                    f_itemlotexpire = ToNull(itemlotExpireFormatted),
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

        public async Task<(int success, int failed, List<string> errors)> ProcessAndSendDataAsync()
        {
            int successCount = 0;
            int failedCount = 0;
            var errors = new List<string>();
            var currentDate = DateHelper.GetCurrentDateChristianEra();

            string query = $@"
                SELECT TOP (@BatchSize)
                   [f_prescriptionno]
      ,[f_seq]
      ,[f_seqmax]
      ,[f_prescriptionnodate]
      ,[f_prioritycode]
      ,[f_prioritydesc]
      ,[f_durationcode]
      ,[f_durationdesc]
      ,[f_orderitemcode]
      ,[f_orderitemname]
      ,[f_Thai_Name]
      ,[f_orderitemgenericname]
      ,[f_orderqty]
      ,[f_orderunitcode]
      ,[f_orderunitdesc]
      ,[f_drugform]
      ,[f_dosage]
      ,[f_dosagedispense]
      ,[f_dosageunit]
      ,[f_drugformname]
      ,[f_ordercreatedate]
      ,[f_ordercreatetime]
      ,[f_orderacceptdate]
      ,[f_orderaccepttime]
      ,[f_orderacceptfromip]
      ,[f_ordertargetdate]
      ,[f_ordertargettime]
      ,[f_itemlotcode]
      ,[f_itemlotexpire]
      ,[f_itemidentify]
      ,[f_noteprocessing]
      ,[f_ipdpt_record_no]
      ,[f_narcoticDrug]
      ,[f_psychotropicDrug]
      ,[f_heighAlertDrug]
      ,[f_useracceptby]
      ,[f_userorderby]
      ,[f_instructioncode]
      ,[f_instructiondesc]
      ,[f_frequencycode]
      ,[f_frequencydesc]
      ,[f_frequencyTime]
      ,[f_sex]
      ,[f_PRN]
      ,[f_patientname]
      ,[f_patientepisodedate]
      ,[f_en]
      ,[f_patientdob]
      ,[f_hn]
      ,[f_daily_dose_flag]
      ,[f_doctorcode]
      ,[f_doctorname]
      ,[f_wardcode]
      ,[f_warddesc]
      ,[f_roomcode]
      ,[f_roomdesc]
      ,[f_fromlocationname]
      ,[f_pharmacylocationcode]
      ,[f_pharmacylocationdesc]
      ,[f_tomachineno]
      ,[f_freetext1]
      ,[f_freetext2]
      ,[f_QR_Code]
      ,[f_freetext4]
      ,[f_bedcode]
      ,[f_beddesc]
      ,[f_referenceCode]
      ,[f_status]
      ,[f_num_type_desc]
      ,[f_num_type_of_daily_dose]
      ,[f_dispensestatus]
      ,[f_opd_adminby]
      ,[f_comment]
      ,[f_ipd_admincontinue]
      ,[f_opd_admindatetime]
      ,[f_opd_adminlocation]
      ,[f_bagspecialdrug]
      ,[f_opd_adminremark]
      ,[f_opd_adminstatus]
      ,[f_lastmodified]
      ,[f_dosevalue]
      ,[f_language]
      ,[f_age]
      ,[f_ordertype]
      ,[f_aux_label_memo]
      ,[f_aux_local_memo]
      ,[f_BarCodeRef]
                FROM tb_thaneshosp_middle
                where 
                    f_dispensestatus='0' and CONVERT(varchar(10),
                    f_prescriptionnodate,112) = @CurrentDate
                ORDER BY f_orderacceptdate";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CurrentDate", currentDate);
                        command.Parameters.AddWithValue("@BatchSize", _batchSize);
                        command.CommandTimeout = 30;
                        command.CommandType = CommandType.Text;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var batchList = new List<PrescriptionBodyRequest>();
                            
                            var batchPrescriptionInfo = new List<(string seq, string prescriptionNo, string prescriptionDate)>();

                            while (await reader.ReadAsync())
                            {
                                string seq = "";
                                string prescriptionNo = "";
                                string prescriptionDateFormatted = "";

                                try
                                {
                                    
                                    seq = reader["f_seq"]?.ToString();
                                    prescriptionNo = reader["f_prescriptionno"]?.ToString();
                                    prescriptionDateFormatted = reader["f_prescriptionnodate"]?.ToString();
                                    // prescriptionDateFormatted = ExtractDate(prescriptionDate);
                                  
                                    if (string.IsNullOrEmpty(prescriptionNo))
                                    {
                                        failedCount++;
                                        continue;
                                    }

                                    var prescriptionBody = BuildPrescriptionBody(reader);
                                    batchList.Add(prescriptionBody);
                                    // ✅ แก้ไข: เรียงลำดับให้ตรงกับที่ประกาศ
                                    batchPrescriptionInfo.Add((seq, prescriptionNo, prescriptionDateFormatted));

                                    if (batchList.Count >= _batchSize)
                                    {
                                        _logger?.LogInfo($"📦 Sending batch ({batchList.Count} items) - Batch full");
                                        var (batchSuccess, batchFailed) = await SendBatchToApiAsync(batchList, batchPrescriptionInfo);
                                        successCount += batchSuccess;
                                        failedCount += batchFailed;

                                        batchList.Clear();
                                        batchPrescriptionInfo.Clear();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    failedCount++;
                                    _logger?.LogError($"❌ Row Error - Rx: {prescriptionNo}", ex);

                                    if (!string.IsNullOrEmpty(seq) && !string.IsNullOrEmpty(prescriptionNo) && !string.IsNullOrEmpty(prescriptionDateFormatted))
                                    {
                                        // ✅ แก้ไข: เรียงลำดับพารามิเตอร์
                                        await UpdateDispenseStatusAsync(seq, prescriptionNo, prescriptionDateFormatted, "3");
                                    }
                                }
                            }

                            if (batchList.Count > 0)
                            {
                                _logger?.LogInfo($"📦 Sending batch ({batchList.Count} items) - Final");
                                var (batchSuccess, batchFailed) = await SendBatchToApiAsync(batchList, batchPrescriptionInfo);
                                successCount += batchSuccess;
                                failedCount += batchFailed;
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger?.LogError("❌ SQL Server Error", ex);
                errors.Add($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger?.LogError("❌ Critical Error", ex);
                errors.Add($"Critical error: {ex.Message}");
            }

            _logger?.LogInfo($"📊 Complete - Success: {successCount}, Failed: {failedCount}");
            return (successCount, failedCount, errors);
        }



        private async Task<(int success, int failed)> SendBatchToApiAsync(
            List<PrescriptionBodyRequest> batchList,
            List<(string seq, string prescriptionNo, string prescriptionDate)> batchInfo)
        {
            int successCount = 0;
            int failedCount = 0;

            try
            {
                var body = new PrescriptionBodyResponse
                {
                    data = batchList.ToArray()
                };

                var json = JsonSerializer.Serialize(body, _jsonOptions);

                _logger?.LogInfo($"📤 Sending {batchList.Count} items ({json.Length / 1024.0:F1} KB)");
                
                if (batchList.Count > 0)
                {
                    var first = batchList[0];
                    _logger?.LogInfo($"   First: Rx={first.f_prescriptionno}, HN={first.f_hn}");
                }

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger?.LogInfo($"✅ Success - {responseContent}");

                    successCount = batchList.Count;
                    await UpdateBatchStatusAsync(batchInfo, "1");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger?.LogError($"❌ API Error {(int)response.StatusCode}: {errorContent.Substring(0, Math.Min(200, errorContent.Length))}");

                    failedCount = batchList.Count;
                    await UpdateBatchStatusAsync(batchInfo, "3");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"❌ Send Exception", ex);
                failedCount = batchList.Count;
                await UpdateBatchStatusAsync(batchInfo, "3");
            }

            return (successCount, failedCount);
        }

        // ✅ แก้ไข: เปลี่ยนเป็น tuple 3 ค่า และใช้ทั้ง 3 คอลัมน์ในการ UPDATE
        private async Task UpdateBatchStatusAsync(
            List<(string seq, string prescriptionNo, string prescriptionDate)> batchInfo,
            string status)
        {
            if (batchInfo == null || batchInfo.Count == 0)
                return;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            foreach (var (seq, prescriptionNo, prescriptionDate) in batchInfo)
                            {
                                
                                string updateQuery = @"
                                    UPDATE tb_thaneshosp_middle
                                    SET f_dispensestatus = @Status  
                                    WHERE f_seq = @Seq
                                      AND f_prescriptionno = @PrescriptionNo 
                                      AND f_prescriptionnodate = @PrescriptionDate";

                                using (var command = new SqlCommand(updateQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@Status", status);
                                    command.Parameters.AddWithValue("@Seq", seq);
                                    command.Parameters.AddWithValue("@PrescriptionNo", prescriptionNo);
                                    command.Parameters.AddWithValue("@PrescriptionDate", prescriptionDate);
                                    command.CommandTimeout = 30;

                                    await command.ExecuteNonQueryAsync();
                                }
                            }

                            await transaction.CommitAsync();
                            _logger?.LogInfo($"✅ Updated {batchInfo.Count} records to status '{status}'");
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            _logger?.LogError($"❌ Transaction rollback", ex);
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"❌ Batch Update Error", ex);
            }
        }

        // ✅ แก้ไข: เปลี่ยนลำดับพารามิเตอร์
        private async Task UpdateDispenseStatusAsync(string seq, string prescriptionNo, string prescriptionDate, string status)
        {
            if (string.IsNullOrEmpty(seq) || string.IsNullOrEmpty(prescriptionNo) || string.IsNullOrEmpty(prescriptionDate))
                return;

            string query = @"
                UPDATE tb_thaneshosp_middle 
                SET f_dispensestatus = @Status
                WHERE f_seq = @Seq
                  AND f_prescriptionno = @PrescriptionNo 
                  AND f_prescriptionnodate = @PrescriptionDate";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Status", status);
                        command.Parameters.AddWithValue("@Seq", seq);
                        command.Parameters.AddWithValue("@PrescriptionNo", prescriptionNo);
                        command.Parameters.AddWithValue("@PrescriptionDate", prescriptionDate);
                        command.CommandTimeout = 10;

                        int affected = await command.ExecuteNonQueryAsync();

                        if (affected > 0)
                            _logger?.LogInfo($"✅ Updated Seq:{seq}, Rx:{prescriptionNo} to status {status}");
                        else
                            _logger?.LogWarning($"⚠️ No record updated for Seq:{seq}, Rx:{prescriptionNo}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"❌ Update Error - Seq:{seq}, Rx={prescriptionNo}", ex);
            }
        }

        public async Task<List<GridViewDataModel>> GetPrescriptionDataAsync(string date = "", string searchText = "")
        {
            var dataList = new List<GridViewDataModel>();
            string queryDate = string.IsNullOrEmpty(date)
                ? DateHelper.GetCurrentDateChristianEra()
                : date.Replace("-", "");
            bool hasSearchText = !string.IsNullOrWhiteSpace(searchText);

            string query = $@"
                SELECT 
                    f_prescriptionno,
                    f_seq,
                    f_seqmax,
                    f_prescriptionnodate,
                    f_patientname,
                    f_hn,
                    f_orderitemname,
                    f_orderqty,
                    f_orderunitdesc,
                    f_dosagedispense,
                    f_dispensestatus
                FROM tb_thaneshosp_middle
                WHERE CONVERT(varchar(10),f_prescriptionnodate,112) = @QueryDate
                AND f_dispensestatus IN ('1', '3')";

            if (hasSearchText)
            {
                query += @" AND (f_hn LIKE @SearchText OR f_prescriptionno LIKE @SearchText)";
            }

            query += @" ORDER BY f_prescriptionno, f_seq";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@QueryDate", queryDate);
                        if (hasSearchText)
                        {
                            command.Parameters.AddWithValue("@SearchText", "%" + searchText.Trim() + "%");
                        }
                        command.CommandTimeout = 30;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                try
                                {
                                    var status = reader["f_dispensestatus"]?.ToString() ?? "";

                                    var item = new GridViewDataModel
                                    {
                                        PrescriptionNo = reader["f_prescriptionno"]?.ToString() ?? "",
                                        Seq = reader["f_seq"]?.ToString() ?? "",
                                        SeqMax = reader["f_seqmax"]?.ToString() ?? "",
                                        Prescriptiondate = reader["f_prescriptionnodate"]?.ToString() ?? "",
                                        PatientName = reader["f_patientname"]?.ToString() ?? "",
                                        HN = reader["f_hn"]?.ToString() ?? "",
                                        ItemName = reader["f_orderitemname"]?.ToString() ?? "",
                                        OrderQty = reader["f_orderqty"]?.ToString() ?? "",
                                        OrderUnit = reader["f_orderunitdesc"]?.ToString() ?? "",
                                        Dosage = reader["f_dosagedispense"]?.ToString() ?? "",
                                        Status = status,
                                    };

                                    dataList.Add(item);
                                }
                                catch (Exception ex)
                                {
                                    _logger?.LogError("Error reading row", ex);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error loading grid data", ex);
            }

            return dataList;
        }

        public async Task<List<PrescriptionBodyRequest>> GetFullPrescriptionDataAsync(
            List<(string prescriptionNo, string prescriptionDate)> prescriptions)
        {
            var dataList = new List<PrescriptionBodyRequest>();

            if (prescriptions == null || prescriptions.Count == 0)
            {
                return dataList;
            }

            string query = @"
                SELECT * FROM tb_thaneshosp_middle
                WHERE f_prescriptionno = @PrescriptionNo
                AND CONVERT(varchar(10),
                    f_prescriptionnodate,112) = @PrescriptionDate
                ORDER BY f_seq";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    foreach (var (prescriptionNo, prescriptionDate) in prescriptions)
                    {
                        try
                        {
                            using (var command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@PrescriptionNo", prescriptionNo);
                                command.Parameters.AddWithValue("@PrescriptionDate", prescriptionDate);
                                command.CommandTimeout = 30;

                                using (var reader = await command.ExecuteReaderAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        try
                                        {
                                            var prescriptionBody = BuildPrescriptionBody(reader);
                                            dataList.Add(prescriptionBody);
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger?.LogError($"Error parsing row for Rx={prescriptionNo}", ex);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError($"Error fetching Rx={prescriptionNo}", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in GetFullPrescriptionDataAsync", ex);
            }

            return dataList;
        }

        private string ExtractDate(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr))
                return "";

            if (DateTime.TryParse(dateStr, out DateTime date))
                return date.ToString("yyyyMMdd");

            return "";
        }

        public static string ExtractDate2(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr))
                return "";

            if (DateTime.TryParse(dateStr, out DateTime date))
                return date.ToString("yyyy-MM-dd");

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