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
    public class FaceRecognizeService : IFaceRecognizeService, IDisposable
    {
        private readonly IUnitOfWorkRepository _unitOfWorkRepository;
        private readonly IImageService _imageService;
        private readonly IFaceDetectionService _faceDetectionService;
        private FaceRecognizeSetting? _faceRecognizeSetting;
        private FaceRecognizer? _faceRecognizer;
        private FaceLandmarker? _faceLandmarker;
        private bool _disposed = false;
        private readonly object _lock = new object(); // สำหรับ thread-safety

        public FaceRecognizeService(
            IUnitOfWorkRepository unitOfWorkRepository,
            IImageService imageService,
            IFaceDetectionService faceDetectionService)
        {
            _unitOfWorkRepository = unitOfWorkRepository ?? throw new ArgumentNullException(nameof(unitOfWorkRepository));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _faceDetectionService = faceDetectionService ?? throw new ArgumentNullException(nameof(faceDetectionService));
        }

        public async Task LoadConfigurationFaceRecognizeAsync()
        {
            _faceRecognizeSetting = await _unitOfWorkRepository.FaceRecognizeSettingRepository.GetActiveFaceRecognizeSettingAsync();

            if (_faceRecognizeSetting == null)
                throw new Exception("ไม่พบการตั้งค่าการรู้จำใบหน้า");

            ConfigureFaceRecognizer();
        }

        private void ConfigureFaceRecognizer()
        {
            if (_faceRecognizeSetting == null)
                throw new ArgumentNullException(nameof(_faceRecognizeSetting));

            lock (_lock)
            {
                _faceRecognizer?.Dispose();
                _faceLandmarker?.Dispose();

                _faceLandmarker = new FaceLandmarker(new FaceLandmarkConfig
                {
                    MarkType = ConvertToMarkType(_faceRecognizeSetting.LandmarkType)
                });

                _faceRecognizer = new FaceRecognizer(new FaceRecognizeConfig
                {
                    FaceType = ConvertToFaceType(_faceRecognizeSetting.RecognizerType)
                });
            }
        }

        private MarkType ConvertToMarkType(string landmarkType)
        {
            return landmarkType.ToLower() switch
            {
                "Light" => MarkType.Light,
                "Mask" => MarkType.Mask,
                "Normal" => MarkType.Normal,
                _ => MarkType.Normal // ค่าเริ่มต้น
            };
        }

        private FaceType ConvertToFaceType(string recognizerType)
        {
            return recognizerType.ToLower() switch
            {
                "Light" => FaceType.Light,
                "Mask" => FaceType.Mask,
                "Normal" => FaceType.Normal,
                _ => FaceType.Normal // ค่าเริ่มต้น
            };
        }

        public async Task<float[]?> ExtractFaceFeatureAsync(byte[] imageData)
        {
            if (_faceRecognizer == null || _faceLandmarker == null)
                await LoadConfigurationFaceRecognizeAsync();

            if (_faceRecognizer == null || _faceLandmarker == null)
                throw new InvalidOperationException("ไม่สามารถเริ่มต้นตัวรู้จำใบหน้าได้");

            // ตรวจจับใบหน้าในรูปภาพก่อน
            var detectionResult = await _faceDetectionService.DetectFromBytesAsync(imageData);

            if (detectionResult.TotalFaces == 0)
                return null; // ไม่พบใบหน้าในรูปภาพ

            // เลือกใบหน้าที่มีความเชื่อมั่นสูงสุด (หรือนำเฉพาะใบหน้าแรก)
            var faceInfo = detectionResult.Faces?
                .OrderByDescending(f => f.Score)
                .First();

            if (faceInfo == null)
                return null; // ไม่พบใบหน้าในรูปภาพ

            return await Task.Run(() =>
            {
                using var image = _imageService.LoadImage(imageData);
                FaceMarkPoint[] landmarks;
                float[] features;

                lock (_lock)
                {
                    // ระบุจุดสำคัญบนใบหน้า
                    landmarks = _faceLandmarker.Mark(image, faceInfo.Value);

                    // สกัดคุณลักษณะใบหน้า (face signature)
                    features = _faceRecognizer.Extract(image, landmarks);
                }

                return features;
            });
        }

        public async Task<float[]?> ExtractFaceFeatureAsync(string imagePath)
        {
            if (!File.Exists(imagePath))
                throw new FileNotFoundException("ไม่พบไฟล์รูปภาพตามเส้นทางที่ระบุ", imagePath);

            byte[] imageData = await File.ReadAllBytesAsync(imagePath);
            return await ExtractFaceFeatureAsync(imageData);
        }

        public async Task<float> CompareFeaturesAsync(float[] features1, float[] features2)
        {
            if (_faceRecognizer == null)
                await LoadConfigurationFaceRecognizeAsync();

            if (_faceRecognizer == null)
                throw new InvalidOperationException("ไม่สามารถเริ่มต้นตัวรู้จำใบหน้าได้");

            return await Task.Run(() =>
            {
                float similarity;

                lock (_lock)
                {
                    // เปรียบเทียบคุณลักษณะใบหน้าสองชุด
                    similarity = _faceRecognizer.Compare(features1, features2);
                }

                return similarity;
            });
        }

        public async Task<bool> IsSamePersonAsync(float[] features1, float[] features2)
        {
            if (_faceRecognizer == null)
                await LoadConfigurationFaceRecognizeAsync();

            if (_faceRecognizer == null)
                throw new InvalidOperationException("ไม่สามารถเริ่มต้นตัวรู้จำใบหน้าได้");

            return await Task.Run(() =>
            {
                bool isSame;

                lock (_lock)
                {
                    // ตรวจสอบว่าใบหน้าสองใบเป็นคนเดียวกันหรือไม่
                    isSame = _faceRecognizer.IsSelf(features1, features2);
                }

                return isSame;
            });
        }

        public async Task<bool> IsSamePersonAsync(float similarity)
        {
            if (_faceRecognizer == null)
                await LoadConfigurationFaceRecognizeAsync();

            if (_faceRecognizer == null)
                throw new InvalidOperationException("ไม่สามารถเริ่มต้นตัวรู้จำใบหน้าได้");

            return await Task.Run(() =>
            {
                bool isSame;

                lock (_lock)
                {
                    // ตรวจสอบว่าค่าความเหมือนสูงพอที่จะบ่งบอกว่าเป็นคนเดียวกันหรือไม่
                    isSame = _faceRecognizer.IsSelf(similarity);
                }

                return isSame;
            });
        }

        public async Task<VerificationResult> VerifyFaceAsync(byte[] knownFaceImage, byte[] unknownFaceImage)
        {
            // สกัดคุณลักษณะของทั้งสองใบหน้า
            var knownFeatures = await ExtractFaceFeatureAsync(knownFaceImage);
            var unknownFeatures = await ExtractFaceFeatureAsync(unknownFaceImage);

            if (knownFeatures == null || unknownFeatures == null)
            {
                return new VerificationResult
                {
                    IsVerified = false,
                    Similarity = 0,
                    ErrorMessage = "ไม่สามารถสกัดคุณลักษณะจากรูปภาพได้"
                };
            }

            // เปรียบเทียบคุณลักษณะ
            var similarity = await CompareFeaturesAsync(knownFeatures, unknownFeatures);
            var isMatch = await IsSamePersonAsync(similarity);

            return new VerificationResult
            {
                IsVerified = isMatch,
                Similarity = similarity,
                ErrorMessage = null
            };
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
                    _faceRecognizer?.Dispose();
                    _faceLandmarker?.Dispose();
                }
                _disposed = true;
            }
        }

        ~FaceRecognizeService()
        {
            Dispose(false);
        }
    }
}
