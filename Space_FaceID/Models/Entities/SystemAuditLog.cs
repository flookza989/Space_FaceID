using Space_FaceID.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Models.Entities
{
    public class SystemAuditLog
    {
        public int Id { get; set; }
        public SystemAuditLogAction Action { get; set; }  // การกระทำ เช่น "UserCreated", "FaceRegistered"
        public int UserId { get; set; }  // ผู้ที่ทำกิจกรรม
        public string Description { get; set; } = null!;  // รายละเอียดของกิจกรรม
        public string Details { get; set; } = null!;  // ข้อมูลเพิ่มเติม (อาจเก็บเป็น JSON)
        public DateTime Timestamp { get; set; }

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}
