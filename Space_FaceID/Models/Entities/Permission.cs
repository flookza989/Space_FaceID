using Space_FaceID.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Models.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public PermissionName Name { get; set; }
        public string? Description { get; set; } // คำอธิบายสิทธิ์
        public PermissionCategory Category { get; set; } // หมวดหมู่ของสิทธิ์ เช่น "User Management", "Face Recognition"
        public bool IsSystem { get; set; }  // สิทธิ์ระบบที่ไม่สามารถลบได้

        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
