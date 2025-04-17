using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!; // ชื่อผู้ใช้
        public string? Email { get; set; } // อีเมล (ถ้ามี)
        public string PasswordHash { get; set; } = null!; // รหัสผ่าน (แฮช)
        public bool IsActive { get; set; } // ผู้ใช้ที่ใช้งานอยู่
        public DateTime LastLogin { get; set; } // วันเวลาที่เข้าสู่ระบบล่าสุด
        public string Role { get; set; } = null!; // บทบาทของผู้ใช้ (Admin, User, etc.)

        // Navigation properties
        public virtual ICollection<FaceData> FaceDatas { get; set; } = new List<FaceData>();
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual FaceData FaceData { get; set; } = null!;
        public virtual UserProfile Profile { get; set; } = null!;
    }
}
