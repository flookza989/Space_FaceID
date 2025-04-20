using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Space_FaceID.Models.Entities;
using Space_FaceID.Repositories.Interfaces;
using Space_FaceID.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Space_FaceID.Services.Implementation
{
    public class CameraService : ICameraService
    {
        private readonly IUnitOfWorkRepository _unitOfWorkRepository;
        private readonly IFaceDetectionService _faceDetectionService;
        private readonly IImageService _imageService;
        private VideoCapture? _capture;
        private Mat _frame;
        private bool _isRunning = false;
        private DispatcherTimer? _captureTimer;
        private BitmapSource? _currentFrame;
        private FaceDetectionSetting? _faceDetectionSetting;

        public CameraService(IUnitOfWorkRepository unitOfWorkRepository, IFaceDetectionService faceDetectionService, IImageService imageService)
        {
            _unitOfWorkRepository = unitOfWorkRepository ?? throw new ArgumentNullException(nameof(unitOfWorkRepository));
            _faceDetectionService = faceDetectionService ?? throw new ArgumentNullException(nameof(faceDetectionService));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));

            _frame = new Mat();
        }

        public bool IsRunning => _isRunning;

        public event EventHandler<BitmapSource>? NewFrameAvailable;

        private async Task LoadFaceDetectionSettingAsync()
        {
            // โหลดการตั้งค่าการตรวจจับใบหน้า
            _faceDetectionSetting = await _unitOfWorkRepository.FaceDetectionSettingRepository.GetActiveFaceDetectionSettingAsync();
            if (_faceDetectionSetting == null)
                throw new Exception("ไม่พบการตั้งค่าการตรวจจับใบหน้า");
        }

        public async Task<bool> StartCameraAsync(int cameraIndex = 0)
        {
            if (_isRunning)
                return true;

            try
            {
                bool isOpened = await Task.Run(() =>
                {
                    _capture = new VideoCapture(cameraIndex);
                    if (!_capture.IsOpened())
                        return false;

                    // ตั้งค่าความละเอียดกล้อง
                    _capture.Set(VideoCaptureProperties.FrameWidth, 640);
                    _capture.Set(VideoCaptureProperties.FrameHeight, 480);

                    return true;
                });

                if (isOpened)
                {
                    _isRunning = true;

                    await LoadFaceDetectionSettingAsync();
                    await _faceDetectionService.LoadConfigurationFaceDetectorAsync();

                    // ใช้ DispatcherTimer แทน Task.Run
                    _captureTimer = new DispatcherTimer(DispatcherPriority.Render)
                    {
                        Interval = TimeSpan.FromMilliseconds(33) // ประมาณ 30fps
                    };

                    _captureTimer.Tick += CaptureFrame;
                    _captureTimer.Start();
                }

                return isOpened;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting camera: {ex.Message}");
                return false;
            }
        }

        public void StopCamera()
        {
            if (!_isRunning)
                return;

            _isRunning = false;

            // หยุด timer
            if (_captureTimer != null && _captureTimer.IsEnabled)
            {
                _captureTimer.Stop();
                _captureTimer.Tick -= CaptureFrame;
                _captureTimer = null;
            }

            // คืนทรัพยากร
            _capture?.Dispose();
            _capture = null;
        }

        private async void CaptureFrame(object? sender, EventArgs e)
        {
            if (!_isRunning || _capture == null)
                return;
            try
            {
                if (_capture.Read(_frame) && !_frame.Empty())
                {
                    // แปลงภาพเป็น BitmapSource
                    var frameImage = _frame.ToBitmapSource();
                    frameImage.Freeze(); // สำคัญมาก: ทำให้สามารถส่งระหว่างเธรดได้

                    if (_faceDetectionSetting != null && _faceDetectionSetting.IsEnabled)
                    {
                        try
                        {
                            // แปลง BitmapSource เป็น byte array โดยใช้ ImageService
                            byte[] imageBytes = _imageService.ConvertBitmapSourceToBytes(frameImage);

                            // ส่งไปให้ FaceDetectionService ประมวลผล
                            var detectionResult = await _faceDetectionService.DetectFromBytesAsync(imageBytes);

                            // สร้างกรอบล้อมรอบใบหน้าที่ตรวจพบบนภาพโดยใช้ ImageService
                            if (detectionResult.TotalFaces > 0)
                            {
                                _currentFrame = _imageService.DrawFacesOnImage(frameImage, detectionResult.Faces);
                            }
                            else
                            {
                                _currentFrame = frameImage;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Face detection error: {ex.Message}");
                            _currentFrame = frameImage;
                        }
                    }
                    else
                    {
                        _currentFrame = frameImage;
                    }

                    NewFrameAvailable?.Invoke(this, _currentFrame);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error capturing frame: {ex.Message}");
            }
        }

        public BitmapSource? CaptureFrame()
        {
            if (!_isRunning || _currentFrame == null)
                return null;

            return _currentFrame;
        }

        public BitmapSource? GetCurrentFrame()
        {
            return _currentFrame;
        }

        public async Task<List<int>> FindConnectedCamerasAsync()
        {
            return await Task.Run(() =>
            {
                List<int> cameras = new List<int>();
                int maxToCheck = 10; // จำนวนกล้องสูงสุดที่จะตรวจสอบ

                for (int i = 0; i < maxToCheck; i++)
                {
                    try
                    {
                        using (VideoCapture vc = new VideoCapture(i))
                        {
                            if (vc.IsOpened())
                            {
                                cameras.Add(i);
                            }
                        }
                    }
                    catch
                    {
                        // ข้ามกล้องที่ไม่สามารถเข้าถึงได้
                        continue;
                    }
                }

                return cameras;
            });
        }

        public void Dispose()
        {
            StopCamera();
            _frame?.Dispose();
        }
    }
}
