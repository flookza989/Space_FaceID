using Space_FaceID.Models.DTOs;
using Space_FaceID.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Space_FaceID.Services.Interfaces
{
    /// <summary>
    /// บริการสำหรับจัดการไฟล์ Excel
    /// </summary>
    public interface IExcelService
    {
        /// <summary>
        /// นำเข้าข้อมูลพนักงานจากไฟล์ Excel
        /// </summary>
        /// <param name="filePath">เส้นทางไฟล์ Excel</param>
        /// <returns>รายการผลลัพธ์การนำเข้าข้อมูล</returns>
        Task<ImportResult> ImportEmployeesFromExcelAsync(string filePath);
        
        /// <summary>
        /// สร้างเทมเพลต Excel สำหรับนำเข้าข้อมูลพนักงาน
        /// </summary>
        /// <param name="filePath">เส้นทางที่ต้องการบันทึกเทมเพลต</param>
        /// <returns>true ถ้าสร้างเทมเพลตสำเร็จ, false ถ้าไม่สำเร็จ</returns>
        Task<bool> CreateEmployeeTemplateAsync(string filePath);
    }
}
