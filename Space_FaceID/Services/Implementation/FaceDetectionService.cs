using Space_FaceID.Helpers.Extensions;
using Space_FaceID.Models.Common;
using Space_FaceID.Models.Entities;
using Space_FaceID.Repositories.Interfaces;
using Space_FaceID.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewFaceCore.Configs;
using ViewFaceCore.Core;
using ViewFaceCore.Model;

namespace Space_FaceID.Services.Implementation
{
    public class FaceDetectionService : IFaceDetectionService, IDisposable
    {
        private readonly IUnitOfWorkRepository _unitOfWorkRepository;
        private readonly IImageService _imageService;
        private FaceDetectionSetting? _faceDetectionSetting;
        private FaceDetectConfig? _config;
        private FaceDetector? _detector;
        private bool _disposed = false;
        private readonly object _lock = new object(); // สำหรับ thread-safety

        public FaceDetectionService(IUnitOfWorkRepository unitOfWorkRepository, IImageService imageService)
        {
            _unitOfWorkRepository = unitOfWorkRepository ?? throw new ArgumentNullException(nameof(unitOfWorkRepository));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        }

        public async Task LoadConfigurationFaceDetectorAsync()
        {
            _faceDetectionSetting = await _unitOfWorkRepository.FaceDetectionSettingRepository.GetActiveFaceDetectionSettingAsync();

            if (_faceDetectionSetting == null)
                throw new Exception("ไม่พบการตั้งค่าการตรวจจับใบหน้า");

            _config = new FaceDetectConfig
            {
                FaceSize = _faceDetectionSetting.FaceSize,
                Threshold = _faceDetectionSetting.DetectionThreshold,
                MaxWidth = _faceDetectionSetting.MaxWidth,
                MaxHeight = _faceDetectionSetting.MaxHeight
            };

            ConfigureDetector();
        }

        private void ConfigureDetector()
        {
            if (_config == null)
                throw new ArgumentNullException(nameof(_config));

            lock (_lock) // ป้องกันกรณีหลาย thread เรียกพร้อมกัน
            {
                _detector?.Dispose(); // ทำลายอินสแตนซ์เดิมก่อนสร้างใหม่
                _detector = new FaceDetector(_config);
            }
        }

        public async Task<FaceDetectionResult> DetectFromFileAsync(string imagePath)
        {
            if (_detector == null)
                await LoadConfigurationFaceDetectorAsync(); // พยายามโหลด config อีกครั้งถ้า detector ยังไม่ได้เริ่มต้น

            if (_detector == null)
                throw new InvalidOperationException("ไม่สามารถเริ่มต้นตัวตรวจจับใบหน้าได้");

            if (!File.Exists(imagePath))
                throw new FileNotFoundException("ไม่พบไฟล์รูปภาพตามเส้นทางที่ระบุ", imagePath);

            return await Task.Run(() =>
            {
                using var image = _imageService.LoadImage(imagePath);
                FaceInfo[] faces;

                lock (_lock) // ป้องกันการเรียกใช้ detector พร้อมกันจากหลาย thread
                {
                    faces = _detector.Detect(image);
                }

                return new FaceDetectionResult
                {
                    TotalFaces = faces.Length,
                    Faces = faces
                };
            });
        }

        public async Task<FaceDetectionResult> DetectFromBytesAsync(byte[] imageData)
        {
            if (_detector == null)
                await LoadConfigurationFaceDetectorAsync(); // พยายามโหลด config อีกครั้งถ้า detector ยังไม่ได้เริ่มต้น

            if (_detector == null)
                throw new InvalidOperationException("ไม่สามารถเริ่มต้นตัวตรวจจับใบหน้าได้");

            if (imageData == null || imageData.Length == 0)
                throw new ArgumentException("ข้อมูลรูปภาพไม่ถูกต้อง");

            return await Task.Run(() =>
            {
                using var image = _imageService.LoadImage(imageData);
                FaceInfo[] faces;

                lock (_lock) // ป้องกันการเรียกใช้ detector พร้อมกันจากหลาย thread
                {
                    faces = _detector.Detect(image);
                }

                return new FaceDetectionResult
                {
                    TotalFaces = faces.Length,
                    Faces = faces
                };
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _detector?.Dispose();
                }
                _disposed = true;
            }
        }

        ~FaceDetectionService()
        {
            Dispose(false);
        }
    }
}