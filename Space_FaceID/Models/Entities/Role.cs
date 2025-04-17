using Space_FaceID.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Models.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public RoleName Name { get; set; }
        public string? Description { get; set; } // คำอธิบายบทบาท
        public bool IsDefault { get; set; }  // บทบาทเริ่มต้นสำหรับผู้ใช้ใหม่
        public bool IsSystem { get; set; }  // บทบาทระบบที่ไม่สามารถลบได้
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
