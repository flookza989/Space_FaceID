using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
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
        private VideoCapture? _capture;
        private Mat _frame;
        private bool _isRunning = false;
        private DispatcherTimer? _captureTimer;
        private BitmapSource? _currentFrame;

        public CameraService()
        {
            _frame = new Mat();
        }

        public bool IsRunning => _isRunning;

        public event EventHandler<BitmapSource>? NewFrameAvailable;

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

        private void CaptureFrame(object? sender, EventArgs e)
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

                    _currentFrame = frameImage;

                    // เรียก event - ตอนนี้เราอยู่บนเธรด UI อยู่แล้ว เพราะใช้ DispatcherTimer
                    NewFrameAvailable?.Invoke(this, frameImage);
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
