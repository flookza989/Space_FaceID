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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        private Task? _captureTask;
        private CancellationTokenSource? _cancellationTokenSource;
        private BitmapSource? _currentFrame;
        private BitmapSource? _originalFrame; // เก็บภาพต้นฉบับก่อนตีกรอบ
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

                    // ใช้ Task แทน DispatcherTimer
                    _cancellationTokenSource = new CancellationTokenSource();
                    var token = _cancellationTokenSource.Token;
                    
                    _captureTask = Task.Run(async () =>
                    {
                        while (!token.IsCancellationRequested && _isRunning)
                        {
                            await CaptureFrameAsync();
                            await Task.Delay(33, token); // ประมาณ 30fps
                        }
                    }, token);
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

            // ยกเลิก task
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            // รอให้ task เสร็จสิ้น
            try
            {
                _captureTask?.Wait(500); // รอสูงสุด 500ms
            }
            catch (Exception ex) when (ex is TaskCanceledException or AggregateException)
            {
                // ไม่ต้องทำอะไร เพราะเรายกเลิก task เอง
                Debug.WriteLine($"Expected task cancellation: {ex.Message}");
            }

            _captureTask = null;

            // คืนทรัพยากร
            _capture?.Dispose();
            _capture = null;
        }

        private async Task CaptureFrameAsync()
        {
            if (!_isRunning || _capture == null)
                return;
            try
            {
                bool success = false;
                // อ่านภาพจากกล้องในเธรดแยกเพื่อไม่ให้ติด UI thread
                await Task.Run(() =>
                {
                    if (_capture != null)
                    {
                        success = _capture.Read(_frame) && !_frame.Empty();
                    }
                });

                if (success)
                {
                    // แปลงภาพเป็น BitmapSource
                    var frameImage = _frame.ToBitmapSource();
                    frameImage.Freeze(); // สำคัญมาก: ทำให้สามารถส่งระหว่างเธรดได้

                    // เก็บภาพต้นฉบับ
                    _originalFrame = frameImage.Clone();
                    _originalFrame.Freeze();

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

                    // การแจ้งเตือนการปรับปรุง UI ต้องทำใน UI thread เท่านั้น
                    await Application.Current.Dispatcher.InvokeAsync(() => 
                    {
                        NewFrameAvailable?.Invoke(this, _currentFrame);
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error capturing frame: {ex.Message}");
            }
        }

        public BitmapSource? CaptureFrame()
        {
            if (!_isRunning || _originalFrame == null)
                return null;

            return _originalFrame;
        }

        public BitmapSource? GetCurrentFrame()
        {
            return _currentFrame;
        }

        public BitmapSource? GetOriginalFrame()
        {
            return _originalFrame;
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
            _cancellationTokenSource?.Dispose();
        }
    }
}
