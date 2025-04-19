using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Models.Entities
{
    public class FaceDetectionSetting
    {
        public int Id { get; set; }
        public int FaceSize { get; set; }  // ขนาดของใบหน้าที่ต้องการตรวจจับ (เช่น 100x100)
        public float DetectionThreshold { get; set; }  // ค่าความไวในการตรวจจับใบหน้า (เช่น 0.5)
        public int MaxWidth { get; set; }  // ความกว้างสูงสุดของภาพที่ใช้ในการตรวจจับ
        public int MaxHeight { get; set; }  // ความสูงสูงสุดของภาพที่ใช้ในการตรวจจับ
        public bool IsEnabled { get; set; }  // เปิดใช้การตรวจจับใบหน้าหรือไม่
        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; } = null!;  // ผู้อัปเดตการตั้งค่าล่าสุด
    }
}
