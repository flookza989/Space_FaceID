using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Models.Entities
{
    public class FaceRecognitionModel
    {
        public int Id { get; set; }
        public string ModelName { get; set; } = null!;  // ชื่อของโมเดล
        public string ModelVersion { get; set; } = null!;  // เวอร์ชันของโมเดล
        public byte[]? ModelData { get; set; }  // ข้อมูลโมเดล (ถ้าเก็บในฐานข้อมูล)
        public string? ModelPath { get; set; }  // เส้นทางไฟล์ของโมเดล (ถ้าเก็บในระบบไฟล์)
        public bool IsActive { get; set; }  // โมเดลที่ใช้งานอยู่ปัจจุบัน
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
    }
}
