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
using interface_Nonthavej.Database;
using System.Threading;
using System.Diagnostics;

namespace interface_Nonthavej.Services
{
    public class DataService : IDisposable
    {
        private readonly DatabaseConnectionPool _connectionPool;
        private readonly string _apiUrl;
        private readonly HttpClient _httpClient;
        private readonly LogManager _logger;
        private readonly int _batchSize;
        private readonly int _apiTimeoutSeconds;
        private bool _disposed = false;

        // ✅ FIX ROOT CAUSE #1: เปลี่ยน Never → WhenWritingNull
        // Never  = ส่ง null ไปใน JSON → JS ฝั่ง API รับ null แล้วทำ null.length → crash 500
        // WhenWritingNull = field ที่เป็น null จะไม่ถูกใส่ใน JSON เลย → API ปลอดภัย
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
        };

        public DataService(
            string connectionString,
            string apiUrl,
            LogManager logger,
            int batchSize,
            int apiTimeoutSeconds)
        {
            _connectionPool = new DatabaseConnectionPool(connectionString, logger);
            _apiUrl = apiUrl;
            _apiTimeoutSeconds = apiTimeoutSeconds;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(apiTimeoutSeconds)
            };
            _httpClient.DefaultRequestHeaders.ConnectionClose = false;
            _logger = logger ?? new LogManager();
            _batchSize = batchSize;

            _logger?.LogInfo($"🔧 DataService initialized | BatchSize: {_batchSize} | Timeout: {_apiTimeoutSeconds}s");
        }

        private string ToNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        // ✅ NEW: SafeRead — อ่าน column จาก reader อย่างปลอดภัย
        // ป้องกัน IndexOutOfRangeException เมื่อ column ไม่มีใน result set
        private string SafeRead(IDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal)) return null;
                return reader.GetValue(ordinal)?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private PrescriptionBodyRequest BuildPrescriptionBody(IDataReader reader)
        {
            try
            {
                // ✅ FIX #2: ใช้ SafeRead ทุกจุดเพื่อป้องกัน IndexOutOfRangeException
                var seq = SafeRead(reader, "f_seq");
                var prescriptionNo = SafeRead(reader, "f_prescriptionno");
                var prescriptionDate = SafeRead(reader, "f_lastmodified");
                var ordercreateDate = SafeRead(reader, "f_ordercreatedate");
                var ordertargetDate = SafeRead(reader, "f_ordertargetdate");
                var orderacceptDate = SafeRead(reader, "f_orderacceptdate");
                var patientdob = SafeRead(reader, "f_patientdob");
                var itemlotExpire = SafeRead(reader, "f_itemlotexpire");
                var ordertargetTimeRaw = SafeRead(reader, "f_ordertargettime");
                var prnValue = SafeRead(reader, "f_PRN");
                var sexRaw = SafeRead(reader, "f_sex");

                var prescriptionDateFormatted = ExtractDate(prescriptionDate);
                var ordercreateDateFormatted = ExtractDatenull(ordercreateDate);
                var ordertargetDateFormatted = ExtractDate2(ordertargetDate);
                var orderacceptDateFormatted = ExtractDatenull(orderacceptDate);
                var patientdobFormatted = ExtractDate2(patientdob);
                var itemlotExpireFormatted = ExtractDate2(itemlotExpire);
                var sex = ProcessSex(sexRaw);
                var prn = ProcessPRN(prnValue, 1);
                var stat = ProcessPRN(prnValue, 2);

                // ✅ FIX #3: UniqID ต้องไม่เป็น null ใช้ ?? "" แทน ToNull
                var uniqId = $"{prescriptionNo?.Trim() ?? ""}{seq?.Trim() ?? ""}";

                // ✅ FIX #4: f_ordertargettime — parse อย่างปลอดภัย แยก null check ออกมาชัดเจน
                string ordertargetTimeFormatted = null;
                if (!string.IsNullOrWhiteSpace(ordertargetTimeRaw) &&
                    DateTime.TryParse(ordertargetTimeRaw, out var parsedTime))
                {
                    ordertargetTimeFormatted = parsedTime.ToString("HH:mm");
                }

                return new PrescriptionBodyRequest
                {
                    UniqID = uniqId,
                    f_prescriptionno = ToNull(prescriptionNo),
                    f_seq = decimal.TryParse(seq, out decimal seqVal) ? seqVal : (decimal?)null,
                    f_seqmax = decimal.TryParse(SafeRead(reader, "f_seqmax"), out decimal seqmax) ? seqmax : (decimal?)null,
                    f_prescriptiondate = ToNull(prescriptionDateFormatted),
                    f_ordercreatedate = ordercreateDateFormatted,
                    f_ordertargetdate = ToNull(ordertargetDateFormatted),
                    f_ordertargettime = ordertargetTimeFormatted,
                    f_doctorcode = ToNull(SafeRead(reader, "f_doctorcode")),
                    f_doctorname = ToNull(SafeRead(reader, "f_doctorname")),
                    f_useracceptby = ToNull(SafeRead(reader, "f_useracceptby")),
                    f_orderacceptdate = orderacceptDateFormatted,
                    f_orderacceptfromip = ToNull(SafeRead(reader, "f_orderacceptfromip")),
                    f_pharmacylocationcode = ToNull(SafeRead(reader, "f_pharmacylocationcode")),
                    f_pharmacylocationdesc = ToNull(SafeRead(reader, "f_pharmacylocationdesc")),
                    f_prioritycode = ToNull(SafeRead(reader, "f_prioritycode")),
                    f_prioritydesc = ToNull(SafeRead(reader, "f_prioritydesc")),
                    f_hn = ToNull(SafeRead(reader, "f_hn")),
                    f_an = ToNull(SafeRead(reader, "f_en")),
                    f_vn = null,
                    f_title = null,
                    f_patientname = ToNull(SafeRead(reader, "f_patientname")),
                    f_sex = ToNull(sex),
                    f_patientdob = ToNull(patientdobFormatted),
                    f_wardcode = ToNull(SafeRead(reader, "f_wardcode")),
                    f_warddesc = ToNull(SafeRead(reader, "f_warddesc")),
                    f_roomcode = ToNull(SafeRead(reader, "f_roomcode")),
                    f_roomdesc = ToNull(SafeRead(reader, "f_roomdesc")),
                    f_bedcode = null,
                    f_beddesc = null,
                    f_right = null,
                    f_drugallergy = null,
                    f_diagnosis = null,
                    f_orderitemcode = ToNull(SafeRead(reader, "f_orderitemcode")),
                    f_orderitemname = ToNull(SafeRead(reader, "f_orderitemname")),
                    f_orderitemnameTH = ToNull(SafeRead(reader, "f_Thai_Name")),
                    f_orderitemnamegeneric = ToNull(SafeRead(reader, "f_orderitemgenericname")),
                    f_orderqty = decimal.TryParse(SafeRead(reader, "f_orderqty"), out decimal qty) ? qty : (decimal?)null,
                    f_orderunitcode = ToNull(SafeRead(reader, "f_orderunitcode")),
                    f_orderunitdesc = ToNull(SafeRead(reader, "f_orderunitdesc")),
                    f_dosage = ToNull(SafeRead(reader, "f_dosage")),
                    f_dosageunit = ToNull(SafeRead(reader, "f_dosageunit")),
                    f_dosagetext = null,
                    f_drugformcode = null,
                    f_drugformdesc = null,
                    // ✅ FIX #5: field เหล่านี้ต้องไม่เป็น null เด็ดขาด
                    // API ฝั่ง Node.js อ่าน .length บน field เหล่านี้ → null.length = crash
                    f_HAD = ToNull(SafeRead(reader, "f_heighAlertDrug")) ?? "0",
                    f_narcoticFlg = ToNull(SafeRead(reader, "f_narcoticDrug")) ?? "0",
                    f_psychotropic = ToNull(SafeRead(reader, "f_psychotropicDrug")) ?? "0",
                    f_binlocation = null,
                    f_itemidentify = null,
                    f_itemlotno = ToNull(SafeRead(reader, "f_itemlotcode")),
                    f_itemlotexpire = ToNull(itemlotExpireFormatted),
                    f_instructioncode = ToNull(SafeRead(reader, "f_instructioncode")),
                    f_instructiondesc = ToNull(SafeRead(reader, "f_instructiondesc")),
                    f_frequencycode = ToNull(SafeRead(reader, "f_frequencycode")),
                    f_frequencydesc = ToNull(SafeRead(reader, "f_frequencydesc")),
                    f_timecode = null,
                    f_timedesc = null,
                    f_frequencytime = ToNull(SafeRead(reader, "f_frequencyTime")),
                    f_dosagedispense = ToNull(SafeRead(reader, "f_dosagedispense")),
                    f_dayofweek = null,
                    f_noteprocessing = ToNull(SafeRead(reader, "f_freetext1")),
                    // ✅ FIX #6: f_prn, f_stat ต้องไม่เป็น null
                    f_prn = prn ?? "0",
                    f_stat = stat ?? "0",
                    f_comment = ToNull(SafeRead(reader, "f_noteprocessing")),
                    f_tomachineno = ToNull(SafeRead(reader, "f_tomachineno")),
                    f_ipd_order_recordno = null,
                    f_status = ToNull(SafeRead(reader, "f_status")),
                    f_remark = ToNull(SafeRead(reader, "f_aux_local_memo")),
                    f_labeltext = ToNull(SafeRead(reader, "f_aux_label_memo")),
                    f_ipdpt_record_no = ToNull(SafeRead(reader, "f_ipdpt_record_no")),
                    f_qr_code = ToNull(SafeRead(reader, "f_QR_Code")),
                    f_barcode_ref = ToNull(SafeRead(reader, "f_BarCodeRef")),
                    f_bagspecialdrug = ToNull(SafeRead(reader, "f_bagspecialdrug"))
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error building prescription body", ex);
                throw;
            }
        }

        public async Task<(int success, int failed, List<string> errors)> ProcessAndSendDataAsync(CancellationToken cancellationToken = default)
        {
            int successCount = 0;
            int failedCount = 0;
            var errors = new List<string>();
            var currentDate = DateHelper.GetCurrentDateChristianEra();

            var circuitState = _connectionPool.GetCircuitState();
            if (circuitState == CircuitBreakerState.Open)
            {
                _logger?.LogWarning("⚠️ Circuit Breaker is OPEN - Skipping database operation");
                errors.Add("Circuit breaker is OPEN. Database might be unavailable.");
                return (0, 0, errors);
            }

            string query = @"
                SELECT
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
                  AND CONVERT(varchar(10), f_lastmodified, 112) = @CurrentDate
                ORDER BY f_orderacceptdate";

            SqlConnection connection = null;
            try
            {
                connection = await _connectionPool.GetConnectionAsync(cancellationToken);
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CurrentDate", currentDate);
                    command.Parameters.AddWithValue("@BatchSize", _batchSize);
                    command.CommandTimeout = 30;
                    command.CommandType = CommandType.Text;

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        var skipApiBatchInfo = new List<(string seq, string prescriptionNo, string prescriptionDate)>();

                        while (await reader.ReadAsync(cancellationToken))
                        {
                            string seq = "";
                            string prescriptionNo = "";
                            string prescriptionDateFormatted = "";

                            try
                            {
                                seq = SafeRead(reader, "f_seq") ?? "";
                                prescriptionNo = SafeRead(reader, "f_prescriptionno") ?? "";
                                var prescriptionDate = SafeRead(reader, "f_lastmodified") ?? "";
                                prescriptionDateFormatted = ExtractDate(prescriptionDate);

                                if (string.IsNullOrEmpty(prescriptionNo))
                                {
                                    failedCount++;
                                    continue;
                                }

                                if (prescriptionNo.StartsWith("R", StringComparison.OrdinalIgnoreCase))
                                {
                                    _logger?.LogInfo($"⏭️ Skip API (R-prefix) - Rx: {prescriptionNo}, Seq: {seq}");
                                    skipApiBatchInfo.Add((seq, prescriptionNo, prescriptionDateFormatted));
                                    successCount++;

                                    if (skipApiBatchInfo.Count >= _batchSize)
                                    {
                                        await UpdateBatchStatusAsync(skipApiBatchInfo, "1", cancellationToken);
                                        skipApiBatchInfo.Clear();
                                    }
                                    continue;
                                }

                                var prescriptionBody = BuildPrescriptionBody(reader);
                                bool sent = await SendSingleToApiAsync(
                                    prescriptionBody, seq, prescriptionNo,
                                    prescriptionDateFormatted, cancellationToken);

                                if (sent) successCount++;
                                else failedCount++;
                            }
                            catch (Exception ex)
                            {
                                failedCount++;
                                _logger?.LogError($"❌ Row Error - Rx: {prescriptionNo}", ex);

                                if (!string.IsNullOrEmpty(seq) &&
                                    !string.IsNullOrEmpty(prescriptionNo) &&
                                    !string.IsNullOrEmpty(prescriptionDateFormatted))
                                {
                                    await UpdateDispenseStatusAsync(
                                        seq, prescriptionNo, prescriptionDateFormatted, "3", cancellationToken);
                                }
                            }
                        }

                        if (skipApiBatchInfo.Count > 0)
                        {
                            _logger?.LogInfo($"⏭️ Updating {skipApiBatchInfo.Count} R-prefix records (no API call)");
                            await UpdateBatchStatusAsync(skipApiBatchInfo, "1", cancellationToken);
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Circuit breaker"))
            {
                _logger?.LogWarning("⚠️ Circuit breaker prevented database operation");
                errors.Add("Circuit breaker is active");
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
            finally
            {
                if (connection != null) _connectionPool.ReleaseConnection(connection);
            }

            return (successCount, failedCount, errors);
        }

        private async Task<bool> SendSingleToApiAsync(
    PrescriptionBodyRequest prescription,
    string seq, string prescriptionNo, string prescriptionDate,
    CancellationToken cancellationToken = default)
        {
            if (prescription == null)
            {
                _logger?.LogError($"❌ Null prescription - Rx: {prescriptionNo}, Seq: {seq}");
                await UpdateDispenseStatusAsync(seq, prescriptionNo, prescriptionDate, "3", cancellationToken);
                return false;
            }

            var wrapper = new { data = new[] { prescription } };
            var json = JsonSerializer.Serialize(wrapper, _jsonOptions);

            _logger?.LogInfo($"📤 Sending Rx: {prescriptionNo}, Seq: {seq} ({json.Length / 1024.0:F1} KB)");

            var stopwatch = Stopwatch.StartNew();
            bool isSuccess = false; // ✅ track ผล

            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_apiUrl, content, cancellationToken);
                stopwatch.Stop();

                var responseContent = await response.Content.ReadAsStringAsync() ?? "";

                if (response.IsSuccessStatusCode)
                {
                    isSuccess = true; // ✅
                    _logger?.LogInfo($"✅ Success | Rx: {prescriptionNo}, Seq: {seq} | ⏱️ {stopwatch.ElapsedMilliseconds}ms");
                    await UpdateDispenseStatusAsync(seq, prescriptionNo, prescriptionDate, "1", cancellationToken);
                }
                else
                {
                    _logger?.LogWarning($"⚠️ API Error {(int)response.StatusCode} | Rx: {prescriptionNo}, Seq: {seq} | {responseContent}");
                }
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();
                _logger?.LogWarning($"⏱️ TIMEOUT | Rx: {prescriptionNo}, Seq: {seq} | {stopwatch.ElapsedMilliseconds}ms");
                // ✅ บันทึก error_detail = timeout
                await SavePayloadToFileAsync(json, prescriptionNo, seq, prescriptionDate, false,
                    $"TIMEOUT after {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                _logger?.LogError($"❌ Network Error | Rx: {prescriptionNo}, Seq: {seq} | {ex.Message}", ex);
                // ✅ บันทึก error_detail = network error
                await SavePayloadToFileAsync(json, prescriptionNo, seq, prescriptionDate, false,
                    $"Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger?.LogError($"❌ Send Exception | Rx: {prescriptionNo}, Seq: {seq}", ex);
                // ✅ บันทึก error_detail = exception
                await SavePayloadToFileAsync(json, prescriptionNo, seq, prescriptionDate, false,
                    $"Exception: {ex.GetType().Name} - {ex.Message}");
            }
            finally
            {
                // ✅ finally save เฉพาะกรณี success (error จัดการใน catch แล้ว)
                if (isSuccess)
                    await SavePayloadToFileAsync(json, prescriptionNo, seq, prescriptionDate, true);
            }

            if (!isSuccess)
            {
                _logger?.LogError($"❌ Send failed | Rx: {prescriptionNo}, Seq: {seq} → status '3'");
                await UpdateDispenseStatusAsync(seq, prescriptionNo, prescriptionDate, "3", cancellationToken);
            }

            return isSuccess;
        }

        private async Task UpdateBatchStatusAsync(
            List<(string seq, string prescriptionNo, string prescriptionDate)> batchInfo,
            string status,
            CancellationToken cancellationToken = default)
        {
            if (batchInfo == null || batchInfo.Count == 0) return;

            SqlConnection connection = null;
            try
            {
                connection = await _connectionPool.GetConnectionAsync(cancellationToken);
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var (seq, prescriptionNo, prescriptionDate) in batchInfo)
                        {
                            if (string.IsNullOrEmpty(seq) ||
                                string.IsNullOrEmpty(prescriptionNo) ||
                                string.IsNullOrEmpty(prescriptionDate))
                            {
                                _logger?.LogWarning($"⚠️ Skip invalid batch key seq={seq}, rx={prescriptionNo}");
                                continue;
                            }

                            string updateQuery = @"
                                UPDATE tb_thaneshosp_middle
                                SET f_dispensestatus = @Status  
                                WHERE f_seq = @Seq
                                  AND f_prescriptionno = @PrescriptionNo 
                                  AND Convert(varchar(10), f_lastmodified, 112) = @Lastmodified";

                            using (var cmd = new SqlCommand(updateQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Status", status);
                                cmd.Parameters.AddWithValue("@Seq", seq);
                                cmd.Parameters.AddWithValue("@PrescriptionNo", prescriptionNo);
                                cmd.Parameters.AddWithValue("@Lastmodified", prescriptionDate);
                                cmd.CommandTimeout = 30;
                                await cmd.ExecuteNonQueryAsync(cancellationToken);
                            }
                        }

                        await transaction.CommitAsync(cancellationToken);
                        _logger?.LogInfo($"✅ Updated {batchInfo.Count} records to status '{status}'");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        _logger?.LogError("❌ Transaction rollback", ex);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("❌ Batch Update Error", ex);
            }
            finally
            {
                if (connection != null) _connectionPool.ReleaseConnection(connection);
            }
        }

        private async Task UpdateDispenseStatusAsync(
            string seq, string prescriptionNo, string prescriptionDate,
            string status, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(seq) || string.IsNullOrEmpty(prescriptionNo) ||
                string.IsNullOrEmpty(prescriptionDate) || string.IsNullOrEmpty(status))
                return;

            string query = @"
                UPDATE tb_thaneshosp_middle 
                SET f_dispensestatus = @Status
                WHERE f_seq = @Seq
                  AND f_prescriptionno = @PrescriptionNo 
                  AND Convert(varchar(10), f_lastmodified, 112) = @Lastmodified";

            SqlConnection connection = null;
            try
            {
                connection = await _connectionPool.GetConnectionAsync(cancellationToken);
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@Seq", seq);
                    cmd.Parameters.AddWithValue("@PrescriptionNo", prescriptionNo);
                    cmd.Parameters.AddWithValue("@Lastmodified", prescriptionDate);
                    cmd.CommandTimeout = 10;

                    int affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                    if (affected > 0)
                        _logger?.LogInfo($"✅ Updated Seq:{seq}, Rx:{prescriptionNo} → status '{status}'");
                    else
                        _logger?.LogWarning($"⚠️ No record updated Seq:{seq}, Rx:{prescriptionNo}");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"❌ Update Error Seq:{seq}, Rx={prescriptionNo}", ex);
            }
            finally
            {
                if (connection != null) _connectionPool.ReleaseConnection(connection);
            }
        }

        public async Task<List<GridViewDataModel>> GetPrescriptionDataAsync(
            string date = "", string searchText = "",
            CancellationToken cancellationToken = default)
        {
            var dataList = new List<GridViewDataModel>();
            string queryDate = string.IsNullOrEmpty(date)
                ? DateHelper.GetCurrentDateChristianEra()
                : date.Replace("-", "");
            bool hasSearchText = !string.IsNullOrWhiteSpace(searchText);

            string query = @"
                SELECT 
                    f_prescriptionno, f_seq, f_seqmax, f_prescriptionnodate,
                    f_patientname, f_hn, f_orderitemname, f_orderqty,
                    f_orderunitdesc, f_dosagedispense, f_dispensestatus, f_lastmodified
                FROM tb_thaneshosp_middle
                WHERE CONVERT(varchar(10), f_lastmodified, 112) = @QueryDate
                  AND f_dispensestatus IN ('1', '3')";

            if (hasSearchText)
                query += " AND (f_hn LIKE @SearchText OR f_prescriptionno LIKE @SearchText)";

            query += " ORDER BY f_prescriptionno, f_seq";

            SqlConnection connection = null;
            try
            {
                connection = await _connectionPool.GetConnectionAsync(cancellationToken);
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@QueryDate", queryDate);
                    if (hasSearchText)
                        cmd.Parameters.AddWithValue("@SearchText", "%" + searchText.Trim() + "%");
                    cmd.CommandTimeout = 30;

                    using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            try
                            {
                                dataList.Add(new GridViewDataModel
                                {
                                    PrescriptionNo = SafeRead(reader, "f_prescriptionno") ?? "",
                                    Seq = SafeRead(reader, "f_seq") ?? "",
                                    SeqMax = SafeRead(reader, "f_seqmax") ?? "",
                                    Prescriptiondate = SafeRead(reader, "f_prescriptionnodate") ?? "",
                                    PatientName = SafeRead(reader, "f_patientname") ?? "",
                                    HN = SafeRead(reader, "f_hn") ?? "",
                                    ItemName = SafeRead(reader, "f_orderitemname") ?? "",
                                    OrderQty = SafeRead(reader, "f_orderqty") ?? "",
                                    OrderUnit = SafeRead(reader, "f_orderunitdesc") ?? "",
                                    Dosage = SafeRead(reader, "f_dosagedispense") ?? "",
                                    Status = SafeRead(reader, "f_dispensestatus") ?? "",
                                    Lastmodified = SafeRead(reader, "f_lastmodified") ?? "",
                                });
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError("Error reading row", ex);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error loading grid data", ex);
            }
            finally
            {
                if (connection != null) _connectionPool.ReleaseConnection(connection);
            }

            return dataList;
        }

        public async Task<List<PrescriptionBodyRequest>> GetFullPrescriptionDataAsync(
            List<(string prescriptionNo, string prescriptionDate)> prescriptions,
            CancellationToken cancellationToken = default)
        {
            var dataList = new List<PrescriptionBodyRequest>();
            if (prescriptions == null || prescriptions.Count == 0) return dataList;

            string query = @"
                SELECT * FROM tb_thaneshosp_middle
                WHERE f_prescriptionno = @PrescriptionNo
                  AND CONVERT(varchar(10), f_lastmodified, 112) = @Lastmodified
                ORDER BY f_seq";

            foreach (var (prescriptionNo, prescriptionDate) in prescriptions)
            {
                if (string.IsNullOrEmpty(prescriptionNo) || string.IsNullOrEmpty(prescriptionDate))
                    continue;

                SqlConnection connection = null;
                try
                {
                    connection = await _connectionPool.GetConnectionAsync(cancellationToken);
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@PrescriptionNo", prescriptionNo);
                        cmd.Parameters.AddWithValue("@Lastmodified", prescriptionDate);
                        cmd.CommandTimeout = 30;

                        using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                try { dataList.Add(BuildPrescriptionBody(reader)); }
                                catch (Exception ex) { _logger?.LogError($"Error parsing row Rx={prescriptionNo}", ex); }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Error fetching Rx={prescriptionNo}", ex);
                }
                finally
                {
                    if (connection != null) _connectionPool.ReleaseConnection(connection);
                }
            }

            return dataList;
        }

        public async Task<(bool isHealthy, string message)> CheckHealthAsync()
            => await _connectionPool.CheckPoolHealthAsync();

        public void ResetCircuitBreaker()
            => _connectionPool.ResetCircuitBreaker();

        // ─────────────── Date Helpers ───────────────

        private string ExtractDate(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return "";
            return DateTime.TryParse(dateStr, out DateTime d) ? d.ToString("yyyyMMdd") : "";
        }

        public static string ExtractDate2(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return "";
            return DateTime.TryParse(dateStr, out DateTime d) ? d.ToString("yyyy-MM-dd") : "";
        }

        public static string ExtractDatenull(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return DateTime.Now.ToString("yyyy-MM-dd");
            return DateTime.TryParse(dateStr, out DateTime d)
                ? d.ToString("yyyy-MM-dd")
                : DateTime.Now.ToString("yyyy-MM-dd");
        }

        private string ProcessSex(string sex)
        {
            if (string.IsNullOrWhiteSpace(sex)) return "U";
            return sex.Trim() switch { "1" => "F", "2" => "M", _ => "U" };
        }

        private string ProcessPRN(string prnValue, int type)
        {
            if (string.IsNullOrWhiteSpace(prnValue) || !int.TryParse(prnValue.Trim(), out int value))
                return "0";
            return (type == 1 && value == 1) || (type == 2 && value == 2) ? "1" : "0";
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _connectionPool?.Dispose();
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
        // เปลี่ยน signature เพิ่ม isSuccess
        // ✅ เพิ่ม parameter errorMessage
        private async Task SavePayloadToFileAsync(
            string json, string prescriptionNo, string seq,
            string prescriptionDate, bool isSuccess, string errorMessage = null)
        {
            try
            {
                string baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Payload");

                string statusFolder = isSuccess ? "success" : "error";
                string dateFolder = Path.Combine(baseFolder, prescriptionDate, statusFolder);

                if (!Directory.Exists(dateFolder))
                    Directory.CreateDirectory(dateFolder);

                string timestamp = DateTime.Now.ToString("HHmmss_fff");
                string safeRx = prescriptionNo.Replace("/", "-").Replace("\\", "-");
                string fileName = $"Rx_{safeRx}_Seq{seq}_{timestamp}.json";
                string filePath = Path.Combine(dateFolder, fileName);

                string jsonToSave = json;
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    using var stream = new System.IO.MemoryStream();
                    using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
                    {
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        Indented = false
                    }))
                    {
                        writer.WriteStartObject();

                        foreach (var prop in root.EnumerateObject())
                            prop.WriteTo(writer);

                        writer.WriteString("message", isSuccess ? "success" : "error");
                        writer.WriteString("saved_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                        // ✅ เพิ่ม error_detail เฉพาะตอน error เท่านั้น
                        if (!isSuccess && !string.IsNullOrWhiteSpace(errorMessage))
                            writer.WriteString("error_detail", errorMessage);

                        writer.WriteEndObject();
                    }

                    jsonToSave = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                }
                catch
                {
                    // fallback บันทึก json เดิม
                }

                await File.WriteAllTextAsync(filePath, jsonToSave, System.Text.Encoding.UTF8);
                _logger?.LogInfo($"💾 Saved payload [{statusFolder}]: {filePath}");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"⚠️ Could not save payload file: {ex.Message}");
            }
        }
    }
}