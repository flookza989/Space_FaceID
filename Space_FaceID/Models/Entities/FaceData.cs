using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Models.Entities
{
    public class FaceData
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Foreign Key
        public byte[] FaceEncoding { get; set; } = null!; // ข้อมูลการเข้ารหัสใบหน้า
        public byte[] FaceImage { get; set; } = null!; // รูปภาพใบหน้า (ถ้าเก็บในฐานข้อมูล)
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}
