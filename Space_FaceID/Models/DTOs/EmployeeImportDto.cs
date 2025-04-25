using System;
using System.ComponentModel.DataAnnotations;

namespace Space_FaceID.Models.DTOs
{
    /// <summary>
    /// DTO สำหรับข้อมูลพนักงานที่นำเข้าจาก Excel
    /// </summary>
    public class EmployeeImportDto
    {
        /// <summary>
        /// ชื่อผู้ใช้ (จำเป็น)
        /// </summary>
        [Required(ErrorMessage = "ชื่อผู้ใช้จำเป็นต้องระบุ")]
        [MinLength(3, ErrorMessage = "ชื่อผู้ใช้ต้องมีความยาวอย่างน้อย 3 ตัวอักษร")]
        [MaxLength(50, ErrorMessage = "ชื่อผู้ใช้ต้องมีความยาวไม่เกิน 50 ตัวอักษร")]
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// รหัสผ่าน (จำเป็น)
        /// </summary>
        [Required(ErrorMessage = "รหัสผ่านจำเป็นต้องระบุ")]
        [MinLength(6, ErrorMessage = "รหัสผ่านต้องมีความยาวอย่างน้อย 6 ตัวอักษร")]
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// อีเมล
        /// </summary>
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        public string? Email { get; set; }
        
        /// <summary>
        /// ชื่อจริง
        /// </summary>
        public string? FirstName { get; set; } = string.Empty;
        
        /// <summary>
        /// นามสกุล
        /// </summary>
        public string? LastName { get; set; } = string.Empty;
        
        /// <summary>
        /// เพศ
        /// </summary>
        public string? Gender { get; set; }
        
        /// <summary>
        /// วันเกิด
        /// </summary>
        public DateTime? DateOfBirth { get; set; }
        
        /// <summary>
        /// เบอร์โทรศัพท์
        /// </summary>
        public string? PhoneNumber { get; set; }
        
        /// <summary>
        /// ที่อยู่
        /// </summary>
        public string? Address { get; set; }
        
        /// <summary>
        /// บทบาท (จำเป็น) - ชื่อบทบาท เช่น "User", "Admin"
        /// </summary>
        [Required(ErrorMessage = "บทบาทจำเป็นต้องระบุ")]
        public string Role { get; set; } = "User";
        
        /// <summary>
        /// สถานะการใช้งาน (true = เปิดใช้งาน, false = ปิดใช้งาน)
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
