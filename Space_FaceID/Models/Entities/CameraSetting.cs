using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Models.Entities
{
    public class CameraSetting
    {
        public int Id { get; set; }
        public int CameraIndex { get; set; }  // ดัชนีกล้อง (เช่น 0 สำหรับกล้องแรก)
        public int FrameRate { get; set; }  // อัตราเฟรม (เช่น 24 fps)
        public int FrameWidth { get; set; }  // ความกว้างของเฟรม
        public int FrameHeight { get; set; }  // ความสูงของเฟรม
        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; } = null!;  // ผู้อัปเดตการตั้งค่าล่าสุด
    }
}
