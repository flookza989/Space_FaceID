using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Models.Enums
{
    public enum RoleName
    {
        Admin = 1,     // ผู้ดูแลระบบที่มีสิทธิ์ทั้งหมด
        User = 2,      // ผู้ใช้งานทั่วไป
        Supervisor = 3 // ผู้ดูแลที่มีสิทธิ์บางส่วน
    }
}
