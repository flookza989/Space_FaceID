using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Models.Entities
{
    public class UserProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; }  // Foreign Key
        public string? FirstName { get; set; } // ชื่อจริง
        public string? LastName { get; set; } // นามสกุล
        public string? PhoneNumber { get; set; } // เบอร์โทรศัพท์
        public string? Address { get; set; } // ที่อยู่
        public DateTime? DateOfBirth { get; set; } // วันเกิด
        public string? Gender { get; set; } // เพศ
        public byte[]? ProfilePicture { get; set; } // รูปโปรไฟล์ (ถ้าเก็บในฐานข้อมูล)

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}
