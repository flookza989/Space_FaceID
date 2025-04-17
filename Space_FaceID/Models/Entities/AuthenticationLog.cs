using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Models.Entities
{
    public class AuthenticationLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; } // Foreign Key (อาจเป็น null ถ้าเป็นการล็อกอินชื่อผู้ใช้ผิด)
        public string Username { get; set; } = null!;  // บันทึกไว้เผื่อกรณี UserId เป็น null
        public bool IsSuccessful { get; set; } // สถานะการล็อกอิน (สำเร็จหรือไม่)
        public DateTime Timestamp { get; set; }
        public string FailureReason { get; set; } = null!;  // กรณีล็อกอินไม่สำเร็จ

        // Navigation property
        public virtual User? User { get; set; }
    }
}
