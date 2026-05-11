using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace interface_Nonthavej.Models
{
    public class PrescriptionBodyRequest
    {
        // ✅ FIX: เพิ่ม [JsonPropertyName] ทุก field
        // เพื่อให้ชื่อ field ที่ส่งไป API ถูกต้องตรงตาม contract เสมอ
        // และป้องกัน API ฝั่ง Node.js อ่านค่าไม่เจอแล้วเกิด undefined.length

        [JsonPropertyName("UniqID")]
        public string UniqID { get; set; }

        [JsonPropertyName("f_prescriptionno")]
        public string f_prescriptionno { get; set; }

        [JsonPropertyName("f_seq")]
        public decimal? f_seq { get; set; }

        [JsonPropertyName("f_seqmax")]
        public decimal? f_seqmax { get; set; }

        [JsonPropertyName("f_prescriptiondate")]
        public string f_prescriptiondate { get; set; }

        [JsonPropertyName("f_ordercreatedate")]
        public string f_ordercreatedate { get; set; }

        [JsonPropertyName("f_ordertargetdate")]
        public string f_ordertargetdate { get; set; }

        [JsonPropertyName("f_ordertargettime")]
        public string f_ordertargettime { get; set; }

        [JsonPropertyName("f_doctorcode")]
        public string f_doctorcode { get; set; }

        [JsonPropertyName("f_doctorname")]
        public string f_doctorname { get; set; }

        [JsonPropertyName("f_useracceptby")]
        public string f_useracceptby { get; set; }

        [JsonPropertyName("f_orderacceptdate")]
        public string f_orderacceptdate { get; set; }

        [JsonPropertyName("f_orderacceptfromip")]
        public string f_orderacceptfromip { get; set; }

        [JsonPropertyName("f_pharmacylocationcode")]
        public string f_pharmacylocationcode { get; set; }

        [JsonPropertyName("f_pharmacylocationdesc")]
        public string f_pharmacylocationdesc { get; set; }

        [JsonPropertyName("f_prioritycode")]
        public string f_prioritycode { get; set; }

        [JsonPropertyName("f_prioritydesc")]
        public string f_prioritydesc { get; set; }

        [JsonPropertyName("f_hn")]
        public string f_hn { get; set; }

        [JsonPropertyName("f_an")]
        public string f_an { get; set; }

        [JsonPropertyName("f_vn")]
        public string f_vn { get; set; }

        [JsonPropertyName("f_title")]
        public string f_title { get; set; }

        [JsonPropertyName("f_patientname")]
        public string f_patientname { get; set; }

        [JsonPropertyName("f_sex")]
        public string f_sex { get; set; }

        [JsonPropertyName("f_patientdob")]
        public string f_patientdob { get; set; }

        [JsonPropertyName("f_wardcode")]
        public string f_wardcode { get; set; }

        [JsonPropertyName("f_warddesc")]
        public string f_warddesc { get; set; }

        [JsonPropertyName("f_roomcode")]
        public string f_roomcode { get; set; }

        [JsonPropertyName("f_roomdesc")]
        public string f_roomdesc { get; set; }

        [JsonPropertyName("f_bedcode")]
        public string f_bedcode { get; set; }

        [JsonPropertyName("f_beddesc")]
        public string f_beddesc { get; set; }

        [JsonPropertyName("f_right")]
        public string f_right { get; set; }

        [JsonPropertyName("f_drugallergy")]
        public string f_drugallergy { get; set; }

        [JsonPropertyName("f_diagnosis")]
        public string f_diagnosis { get; set; }

        [JsonPropertyName("f_orderitemcode")]
        public string f_orderitemcode { get; set; }

        [JsonPropertyName("f_orderitemname")]
        public string f_orderitemname { get; set; }

        [JsonPropertyName("f_orderitemnameTH")]
        public string f_orderitemnameTH { get; set; }

        [JsonPropertyName("f_orderitemnamegeneric")]
        public string f_orderitemnamegeneric { get; set; }

        [JsonPropertyName("f_orderqty")]
        public decimal? f_orderqty { get; set; }

        [JsonPropertyName("f_orderunitcode")]
        public string f_orderunitcode { get; set; }

        [JsonPropertyName("f_orderunitdesc")]
        public string f_orderunitdesc { get; set; }

        [JsonPropertyName("f_dosage")]
        public string f_dosage { get; set; }

        [JsonPropertyName("f_dosageunit")]
        public string f_dosageunit { get; set; }

        [JsonPropertyName("f_dosagetext")]
        public string f_dosagetext { get; set; }

        [JsonPropertyName("f_drugformcode")]
        public string f_drugformcode { get; set; }

        [JsonPropertyName("f_drugformdesc")]
        public string f_drugformdesc { get; set; }

        // ✅ FIX: f_HAD, f_narcoticFlg, f_psychotropic มี default "0" แต่ถ้าเป็น null
        // JsonIgnoreCondition.WhenWritingNull จะไม่ส่งไป → API อาจ error
        // แก้โดยให้ default เป็น "0" ใน model ด้วย เพื่อ safety 2 ชั้น
        [JsonPropertyName("f_HAD")]
        public string f_HAD { get; set; } = "0";

        [JsonPropertyName("f_narcoticFlg")]
        public string f_narcoticFlg { get; set; } = "0";

        [JsonPropertyName("f_psychotropic")]
        public string f_psychotropic { get; set; } = "0";

        [JsonPropertyName("f_binlocation")]
        public string f_binlocation { get; set; }

        [JsonPropertyName("f_itemidentify")]
        public string f_itemidentify { get; set; }

        [JsonPropertyName("f_itemlotno")]
        public string f_itemlotno { get; set; }

        [JsonPropertyName("f_itemlotexpire")]
        public string f_itemlotexpire { get; set; }

        [JsonPropertyName("f_instructioncode")]
        public string f_instructioncode { get; set; }

        [JsonPropertyName("f_instructiondesc")]
        public string f_instructiondesc { get; set; }

        [JsonPropertyName("f_frequencycode")]
        public string f_frequencycode { get; set; }

        [JsonPropertyName("f_frequencydesc")]
        public string f_frequencydesc { get; set; }

        [JsonPropertyName("f_timecode")]
        public string f_timecode { get; set; }

        [JsonPropertyName("f_timedesc")]
        public string f_timedesc { get; set; }

        [JsonPropertyName("f_frequencytime")]
        public string f_frequencytime { get; set; }

        [JsonPropertyName("f_dosagedispense")]
        public string f_dosagedispense { get; set; }

        [JsonPropertyName("f_dayofweek")]
        public string f_dayofweek { get; set; }

        [JsonPropertyName("f_noteprocessing")]
        public string f_noteprocessing { get; set; }

        // ✅ FIX: f_prn, f_stat มี default "0" เพื่อป้องกัน API อ่าน undefined
        [JsonPropertyName("f_prn")]
        public string f_prn { get; set; } = "0";

        [JsonPropertyName("f_stat")]
        public string f_stat { get; set; } = "0";

        [JsonPropertyName("f_comment")]
        public string f_comment { get; set; }

        [JsonPropertyName("f_tomachineno")]
        public string f_tomachineno { get; set; }

        [JsonPropertyName("f_ipd_order_recordno")]
        public string f_ipd_order_recordno { get; set; }

        [JsonPropertyName("f_status")]
        public string f_status { get; set; }

        [JsonPropertyName("f_remark")]
        public string f_remark { get; set; }

        [JsonPropertyName("f_labeltext")]
        public string f_labeltext { get; set; }

        [JsonPropertyName("f_ipdpt_record_no")]
        public string f_ipdpt_record_no { get; set; }

        [JsonPropertyName("f_qr_code")]
        public string f_qr_code { get; set; }

        [JsonPropertyName("f_barcode_ref")]
        public string f_barcode_ref { get; set; }

        [JsonPropertyName("f_bagspecialdrug")]
        public string f_bagspecialdrug { get; set; }
    }

    public class PrescriptionBodyResponse
    {
        [JsonPropertyName("data")]
        public PrescriptionBodyRequest[] data { get; set; }
    }

    public class GridViewDataModel
    {
        public string PrescriptionNo { get; set; }
        public string Seq { get; set; }
        public string SeqMax { get; set; }
        public string Prescriptiondate { get; set; }
        public string PatientName { get; set; }
        public string HN { get; set; }
        public string ItemName { get; set; }
        public string OrderQty { get; set; }
        public string OrderUnit { get; set; }
        public string Dosage { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public string Lastmodified { get; set; }
    }
}