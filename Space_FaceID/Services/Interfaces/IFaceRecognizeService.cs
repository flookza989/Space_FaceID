using Space_FaceID.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Services.Interfaces
{
    public interface IFaceRecognizeService
    {
        /// <summary>
        /// โหลดการตั้งค่าจากฐานข้อมูลและปรับใช้
        /// </summary>
        Task LoadConfigurationFaceRecognizeAsync();

        /// <summary>
        /// สกัดคุณลักษณะใบหน้าจากข้อมูลรูปภาพ
        /// </summary>
        /// <param name="imageData">ข้อมูลรูปภาพ</param>
        /// <returns>ลายนิ้วมือใบหน้า หรือ null ถ้าไม่พบใบหน้า</returns>
        Task<float[]?> ExtractFaceFeatureAsync(byte[] imageData);

        /// <summary>
        /// สกัดคุณลักษณะใบหน้าจากไฟล์รูปภาพ
        /// </summary>
        /// <param name="imagePath">เส้นทางไฟล์รูปภาพ</param>
        /// <returns>ลายนิ้วมือใบหน้า หรือ null ถ้าไม่พบใบหน้า</returns>
        Task<float[]?> ExtractFaceFeatureAsync(string imagePath);

        /// <summary>
        /// เปรียบเทียบคุณลักษณะใบหน้าสองชุด
        /// </summary>
        /// <param name="features1">ลายนิ้วมือใบหน้าชุดที่ 1</param>
        /// <param name="features2">ลายนิ้วมือใบหน้าชุดที่ 2</param>
        /// <returns>ค่าความเหมือน (0.0-1.0)</returns>
        Task<float> CompareFeaturesAsync(float[] features1, float[] features2);

        /// <summary>
        /// ตรวจสอบว่าใบหน้าสองใบเป็นคนเดียวกันหรือไม่
        /// </summary>
        /// <param name="features1">ลายนิ้วมือใบหน้าชุดที่ 1</param>
        /// <param name="features2">ลายนิ้วมือใบหน้าชุดที่ 2</param>
        /// <returns>true ถ้าเป็นคนเดียวกัน, false ถ้าไม่ใช่</returns>
        Task<bool> IsSamePersonAsync(float[] features1, float[] features2);

        /// <summary>
        /// ตรวจสอบว่าค่าความเหมือนสูงพอที่จะบ่งบอกว่าเป็นคนเดียวกันหรือไม่
        /// </summary>
        /// <param name="similarity">ค่าความเหมือน</param>
        /// <returns>true ถ้าเป็นคนเดียวกัน, false ถ้าไม่ใช่</returns>
        Task<bool> IsSamePersonAsync(float similarity);

        /// <summary>
        /// ยืนยันตัวตนโดยเปรียบเทียบใบหน้าที่รู้จักกับใบหน้าที่ไม่รู้จัก
        /// </summary>
        /// <param name="knownFaceImage">ข้อมูลรูปภาพของใบหน้าที่รู้จัก</param>
        /// <param name="unknownFaceImage">ข้อมูลรูปภาพของใบหน้าที่ต้องการยืนยัน</param>
        /// <returns>ผลลัพธ์การยืนยันตัวตน</returns>
        Task<VerificationResult> VerifyFaceAsync(byte[] knownFaceImage, byte[] unknownFaceImage);
    }
}
