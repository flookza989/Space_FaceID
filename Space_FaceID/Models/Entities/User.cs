﻿using System;
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
        public int RoleId { get; set; } // Foreign Key (บทบาทของผู้ใช้)
        public DateTime? LastLogin { get; set; } // วันเวลาที่เข้าสู่ระบบล่าสุด

        // Navigation properties
        public virtual ICollection<FaceData> FaceDatas { get; set; } = new List<FaceData>(); // ข้อมูลใบหน้าของผู้ใช้
        public virtual UserProfile Profile { get; set; } = null!; // ข้อมูลโปรไฟล์ผู้ใช้
        public virtual Role Role { get; set; } = null!; // บทบาทของผู้ใช้
    }
}
