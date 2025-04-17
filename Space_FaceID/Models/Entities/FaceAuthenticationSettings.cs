using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Models.Entities
{
    public class FaceAuthenticationSettings
    {
        public int Id { get; set; }
        public float MatchThreshold { get; set; }  // ค่าความเหมือนขั้นต่ำที่ยอมรับได้ (เช่น 0.6)
        public bool RequireLivenessCheck { get; set; }  // ต้องการการตรวจสอบว่าเป็นใบหน้าจริงไม่ใช่รูปถ่าย
        public int MaxAttempts { get; set; }  // จำนวนครั้งสูงสุดที่ล้มเหลวก่อนถูกล็อค
        public bool IsEnabled { get; set; }  // เปิดใช้ระบบยืนยันตัวตนด้วยใบหน้าหรือไม่
        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; } = null!;  // ผู้อัปเดตการตั้งค่าล่าสุด
    }
}
