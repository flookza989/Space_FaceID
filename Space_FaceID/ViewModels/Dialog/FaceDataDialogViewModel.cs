using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Space_FaceID.Models.Entities;
using Space_FaceID.Repositories.Interfaces;
using Space_FaceID.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Space_FaceID.ViewModels.Dialog
{
    public partial class FaceDataDialogViewModel : ObservableObject, IDisposable
    {
        private readonly IUnitOfWorkRepository _unitOfWorkRepository;
        private readonly ICameraService _cameraService;
        private readonly IFaceDetectionService _faceDetectionService;
        private readonly IFaceRecognizeService _faceRecognizeService;
        private readonly IImageService _imageService;

        [ObservableProperty]
        private User? _user;

        [ObservableProperty]
        private BitmapSource? _cameraPreview;

        [ObservableProperty]
        private BitmapSource? _capturedFace;

        [ObservableProperty]
        private BitmapSource? _originalCapturedImage;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _loadingMessage = string.Empty;

        [ObservableProperty]
        private bool _isCameraActive = false;

        [ObservableProperty]
        private bool _isFaceDetected = false;

        [ObservableProperty]
        private bool _isFaceCaptured = false;

        [ObservableProperty]
        private bool _canSave = false;

        [ObservableProperty]
        private List<int> _availableCameras = new List<int>();

        [ObservableProperty]
        private int _selectedCameraIndex = 0;

        // คำสั่งสำหรับบันทึกข้อมูล
        [RelayCommand]
        private async Task Save()
        {
            // 1. ตรวจสอบข้อมูลเบื้องต้น
            if (User == null)
            {
                MessageBox.Show("ไม่พบข้อมูลผู้ใช้ กรุณาเริ่มต้นใหม่", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsFaceCaptured || OriginalCapturedImage == null)
            {
                MessageBox.Show("กรุณาถ่ายภาพใบหน้าก่อนบันทึก", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. ยืนยันการบันทึก
            var confirmResult = MessageBox.Show(
                $"ต้องการบันทึกข้อมูลใบหน้าของ {User.Profile?.FirstName} {User.Profile?.LastName} ใช่หรือไม่?",
                "ยืนยันการบันทึก",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                IsLoading = true;
                LoadingMessage = "กำลังบันทึกข้อมูลใบหน้า...";

                // 3. ตรวจสอบว่ามีใบหน้านี้ในระบบแล้วหรือไม่ (ตรวจสอบความซ้ำซ้อน)
                LoadingMessage = "กำลังตรวจสอบข้อมูลเดิม...";
                var existingFaceDatas = await _unitOfWorkRepository.FaceDataRepository.GetFaceDatasByUserIdAsync(User.Id);
                if (existingFaceDatas.Count >= 5) // สมมติว่ามีการจำกัดไม่ให้มีภาพใบหน้ามากเกิน 5 ภาพต่อคน
                {
                    var confirmOverwrite = MessageBox.Show(
                        $"{User.Profile?.FirstName} {User.Profile?.LastName} มีข้อมูลใบหน้าในระบบแล้ว {existingFaceDatas.Count} ภาพ\nซึ่งถึงจำนวนสูงสุดที่แนะนำ (5 ภาพ)\n\nต้องการลบภาพเก่าที่สุดและบันทึกภาพใหม่แทนหรือไม่?",
                        "ข้อมูลใบหน้าเกินกำหนด",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (confirmOverwrite == MessageBoxResult.Yes)
                    {
                        // ลบข้อมูลใบหน้าเก่าที่สุด (เรียงตามวันที่สร้าง)
                        LoadingMessage = "กำลังลบข้อมูลใบหน้าเก่า...";
                        var oldestFace = existingFaceDatas.OrderBy(f => f.CreatedAt).First();
                        await _unitOfWorkRepository.FaceDataRepository.RemoveAsync(oldestFace);
                    }
                    else
                    {
                        MessageBox.Show("ยกเลิกการบันทึกข้อมูลใบหน้าใหม่", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                // 4. แปลงภาพเป็น byte array
                LoadingMessage = "กำลังประมวลผลภาพ...";
                byte[] faceImageBytes = _imageService.ConvertBitmapSourceToBytes(OriginalCapturedImage);

                // 5. สกัดคุณลักษณะใบหน้า (Face Encoding)
                LoadingMessage = "กำลังสกัดลักษณะใบหน้า...";
                var faceFeatures = await _faceRecognizeService.ExtractFaceFeatureAsync(faceImageBytes);
                if (faceFeatures == null)
                {
                    MessageBox.Show("ไม่สามารถสกัดลักษณะใบหน้าได้\nกรุณาตรวจสอบว่าภาพมีคุณภาพดีและมีใบหน้าที่ชัดเจน", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 6. ตรวจสอบขนาดภาพ
                if (faceImageBytes.Length > 1024 * 1024) // ถ้าภาพมีขนาดใหญ่กว่า 1MB
                {
                    LoadingMessage = "ภาพมีขนาดใหญ่ อาจต้องใช้เวลาในการบันทึก...";
                    // ไม่มีเมธอด ResizeImageBytes ใน IImageService 
                    // อาจต้องเพิ่มเมธอดในอนาคต
                    Debug.WriteLine($"Large image: {faceImageBytes.Length / 1024} KB");
                }

                // 7. สร้างข้อมูลใบหน้าใหม่
                LoadingMessage = "กำลังบันทึกข้อมูลลงฐานข้อมูล...";
                var faceData = new FaceData
                {
                    UserId = User.Id,
                    FaceImage = faceImageBytes,
                    FaceEncoding = GetByteArrayFromFloatArray(faceFeatures),
                    CreatedAt = DateTime.Now,
                };

                // 8. บันทึกลงฐานข้อมูล
                await _unitOfWorkRepository.FaceDataRepository.AddAsync(faceData);

                // 9. แสดงสถานะการบันทึก
                MessageBox.Show(
                    $"บันทึกข้อมูลใบหน้าของ {User.Profile?.FirstName} {User.Profile?.LastName} เรียบร้อยแล้ว",
                    "สำเร็จ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // 10. ตั้งค่า DialogResult เป็น true เพื่อแจ้งว่าบันทึกสำเร็จ
                await SetResultAsync(true);
            }
            catch (Exception ex)
            {
                // จัดการกับข้อผิดพลาดที่เกิดขึ้น
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nรายละเอียดเพิ่มเติม: {ex.InnerException.Message}";
                }

                MessageBox.Show(
                    $"เกิดข้อผิดพลาดในการบันทึกข้อมูล:\n{errorMessage}",
                    "ข้อผิดพลาด",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                // บันทึกข้อผิดพลาดลงใน log สำหรับการตรวจสอบ
                Debug.WriteLine($"Error saving face data: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // คำสั่งสำหรับยกเลิก
        [RelayCommand]
        private async Task Cancel()
        {
            // หยุดกล้อง
            StopCamera();
            // ตั้งค่า DialogResult เป็น false
            await SetResultAsync(false);
        }

        // คำสั่งสำหรับเริ่มใช้งานกล้อง
        [RelayCommand]
        private async Task StartCamera()
        {
            if (SelectedCameraIndex < 0 || SelectedCameraIndex >= AvailableCameras.Count)
            {
                MessageBox.Show("ไม่พบกล้องที่เลือก กรุณาตรวจสอบการเชื่อมต่อกล้อง", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                IsLoading = true;
                LoadingMessage = "กำลังเปิดกล้อง...";

                // ปิดกล้องที่กำลังใช้งานอยู่ก่อน (ถ้ามี)
                if (_cameraService.IsRunning)
                {
                    _cameraService.StopCamera();
                }

                // เปิดกล้องใหม่
                bool success = await _cameraService.StartCameraAsync(AvailableCameras[SelectedCameraIndex]);

                if (success)
                {
                    IsCameraActive = true;

                    // ลงทะเบียนรับภาพจากกล้อง
                    _cameraService.NewFrameAvailable += CameraService_NewFrameAvailable;
                }
                else
                {
                    MessageBox.Show("ไม่สามารถเปิดกล้องได้ กรุณาตรวจสอบการเชื่อมต่อกล้อง", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการเปิดกล้อง: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // คำสั่งสำหรับหยุดใช้งานกล้อง
        [RelayCommand]
        private void StopCamera()
        {
            // ยกเลิกการรับภาพจากกล้อง
            _cameraService.NewFrameAvailable -= CameraService_NewFrameAvailable;

            // หยุดกล้อง
            _cameraService.StopCamera();

            IsCameraActive = false;
        }

        // คำสั่งสำหรับถ่ายภาพ
        [RelayCommand]
        private async Task CaptureImage()
        {
            if (!IsCameraActive)
            {
                MessageBox.Show("กรุณาเปิดกล้องก่อนถ่ายภาพ", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                LoadingMessage = "กำลังถ่ายภาพและตรวจจับใบหน้า...";

                // ถ่ายภาพจากกล้อง (จะได้ภาพต้นฉบับไม่มีกรอบตามที่ปรับปรุง CameraService)
                var frame = _cameraService.CaptureFrame();

                if (frame == null)
                {
                    MessageBox.Show("ไม่สามารถถ่ายภาพได้ กรุณาลองใหม่อีกครั้ง", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // เก็บภาพต้นฉบับไว้
                OriginalCapturedImage = frame;

                // แปลงภาพเป็น byte array สำหรับตรวจสอบใบหน้า
                byte[] imageBytes = _imageService.ConvertBitmapSourceToBytes(frame);

                // ตรวจจับใบหน้า
                var faceDetectionResult = await _faceDetectionService.DetectFromBytesAsync(imageBytes);

                if (faceDetectionResult.Faces == null || faceDetectionResult.Faces.Length == 0)
                {
                    MessageBox.Show("ไม่พบใบหน้าในภาพ กรุณาลองใหม่อีกครั้ง", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // วาดกรอบใบหน้าที่ตรวจพบ (เก็บไว้ใน CapturedFace สำหรับการประมวลผลภายใน)
                var imageWithFaces = _imageService.DrawFacesOnImage(frame, faceDetectionResult.Faces);
                CapturedFace = imageWithFaces;

                // แสดงภาพที่ถ่ายได้ (ภาพต้นฉบับไม่มีกรอบ)
                IsFaceCaptured = true;
                CanSave = true;

                // หยุดกล้องหลังจากถ่ายภาพ
                StopCamera();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการถ่ายภาพ: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // คำสั่งสำหรับถ่ายภาพใหม่
        [RelayCommand]
        private void RetakeImage()
        {
            // ล้างภาพที่ถ่ายไว้ก่อนหน้า
            CapturedFace = null;
            OriginalCapturedImage = null;
            IsFaceCaptured = false;
            CanSave = false;

            // เปิดกล้องใหม่
            StartCameraCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadCameras()
        {
            try
            {
                IsLoading = true;
                LoadingMessage = "กำลังค้นหากล้อง...";

                // ค้นหากล้องที่เชื่อมต่ออยู่
                var cameras = await _cameraService.FindConnectedCamerasAsync();

                if (cameras.Count > 0)
                {
                    AvailableCameras = cameras;
                    SelectedCameraIndex = 0;
                }
                else
                {
                    MessageBox.Show("ไม่พบกล้องที่เชื่อมต่อ กรุณาเชื่อมต่อกล้องก่อนใช้งาน", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการค้นหากล้อง: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Task SetResultAsync(bool result)
        {
            // สำหรับปิดไดอะล็อก และส่งผลลัพธ์กลับ
            // ปิด Dialog โดยส่งค่า true กลับ (บันทึกสำเร็จ)
            DialogHost.Close("RootDialog", true);
            return Task.CompletedTask;
        }

        private void CameraService_NewFrameAvailable(object? sender, BitmapSource e)
        {
            // เมื่อได้รับภาพใหม่จากกล้อง
            CameraPreview = e;

            // ตรวจสอบการตรวจพบใบหน้าแบบเรียลไทม์ (อาจจะเพิ่มลงในอนาคต)
        }

        private byte[] GetByteArrayFromFloatArray(float[] floatArray)
        {
            // แปลง float array เป็น byte array
            byte[] byteArray = new byte[floatArray.Length * 4];
            Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }

        public FaceDataDialogViewModel(
            IUnitOfWorkRepository unitOfWorkRepository,
            ICameraService cameraService,
            IFaceDetectionService faceDetectionService,
            IFaceRecognizeService faceRecognizeService,
            IImageService imageService)
        {
            _unitOfWorkRepository = unitOfWorkRepository ?? throw new ArgumentNullException(nameof(unitOfWorkRepository));
            _cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
            _faceDetectionService = faceDetectionService ?? throw new ArgumentNullException(nameof(faceDetectionService));
            _faceRecognizeService = faceRecognizeService ?? throw new ArgumentNullException(nameof(faceRecognizeService));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        }

        public void Initialize(User user)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            // โหลดรายการกล้อง
            LoadCamerasCommand.Execute(null);
        }

        public void Dispose()
        {
            // หยุดกล้อง
            if (_cameraService.IsRunning)
            {
                _cameraService.NewFrameAvailable -= CameraService_NewFrameAvailable;
                _cameraService.StopCamera();
            }

            GC.SuppressFinalize(this);
        }
    }
}
