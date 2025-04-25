using Space_FaceID.Models.Entities;
using System;
using System.Collections.Generic;

namespace Space_FaceID.Models.DTOs
{
    /// <summary>
    /// DTO สำหรับผลลัพธ์การนำเข้าข้อมูล
    /// </summary>
    public class ImportResult
    {
        /// <summary>
        /// รายการผู้ใช้ที่นำเข้าสำเร็จ
        /// </summary>
        public List<User> SuccessfulImports { get; set; } = new List<User>();
        
        /// <summary>
        /// รายการผิดพลาดที่เกิดขึ้นระหว่างการนำเข้า
        /// </summary>
        public List<ImportError> Errors { get; set; } = new List<ImportError>();
        
        /// <summary>
        /// จำนวนแถวทั้งหมดที่พยายามนำเข้า (รวมหัวตาราง)
        /// </summary>
        public int TotalRows { get; set; }
        
        /// <summary>
        /// จำนวนแถวที่นำเข้าจริง (ไม่รวมหัวตารางและหมายเหตุ)
        /// </summary>
        public int ActualRows { get; set; }
        
        /// <summary>
        /// จำนวนรายการที่นำเข้าสำเร็จ
        /// </summary>
        public int SuccessCount => SuccessfulImports.Count;
        
        /// <summary>
        /// จำนวนรายการที่เกิดข้อผิดพลาด
        /// </summary>
        public int ErrorCount => Errors.Count;
        
        /// <summary>
        /// เวลาที่เริ่มการนำเข้า
        /// </summary>
        public DateTime ImportStartTime { get; set; }
        
        /// <summary>
        /// เวลาที่สิ้นสุดการนำเข้า
        /// </summary>
        public DateTime ImportEndTime { get; set; }
        
        /// <summary>
        /// เวลาที่ใช้ในการนำเข้าทั้งหมด (วินาที)
        /// </summary>
        public double TotalTimeSeconds => (ImportEndTime - ImportStartTime).TotalSeconds;
    }
    
    /// <summary>
    /// รายละเอียดข้อผิดพลาดในการนำเข้าข้อมูล
    /// </summary>
    public class ImportError
    {
        /// <summary>
        /// แถวที่เกิดข้อผิดพลาด (เริ่มจาก 1)
        /// </summary>
        public int RowNumber { get; set; }
        
        /// <summary>
        /// ข้อความแสดงข้อผิดพลาด
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
        
        /// <summary>
        /// ชื่อผู้ใช้ที่พยายามนำเข้า (ถ้ามี)
        /// </summary>
        public string? Username { get; set; }
    }
}