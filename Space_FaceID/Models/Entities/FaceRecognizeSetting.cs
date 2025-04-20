using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Models.Entities
{
    public class FaceRecognizeSetting
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;  // ชื่อการตั้งค่า
        public string RecognizerType { get; set; } = null!;  // ประเภทของการจดจำใบหน้า (เช่น Normal, Light, Mask)
        public string LandmarkType { get; set; } = null!;  // ประเภทของ Landmark ที่ใช้ (เช่น Normal, Light, Mask)
        public float RecognizeThreshold { get; set; }  // ค่าความไวในการจดจำใบหน้า (เช่น 0.62f)
        public bool IsEnabled { get; set; }  // การตั้งค่านี้ถูกใช้งานอยู่หรือไม่
        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; } = null!;  // ผู้อัปเดตการตั้งค่าล่าสุด 
    }
}
