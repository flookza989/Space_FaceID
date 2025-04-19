using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Space_FaceID.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
using Space_FaceID.Models.Entities;
using Space_FaceID.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using ViewFaceCore.Configs;
using ViewFaceCore.Core;

namespace Space_FaceID.ViewModels
{
    public partial class DashboardViewModel : ObservableObject, IDisposable
    {
        private readonly ICameraService _cameraService;
        private readonly IServiceProvider _serviceProvider;

        // ไม่ใช้ [ObservableProperty] กับ CameraImageSource ในที่นี้
        private BitmapSource? _cameraImageSource;
        public BitmapSource? CameraImageSource
        {
            get => _cameraImageSource;
            set
            {
                // ถ้าเป็นค่าเดียวกันให้ข้าม
                if (_cameraImageSource == value) return;

                _cameraImageSource = value;
                OnPropertyChanged(nameof(CameraImageSource));
            }
        }

        [ObservableProperty]
        private bool _isFaceDetected;

        [ObservableProperty]
        private double _faceScaleX = 1.0;

        [ObservableProperty]
        private double _faceScaleY = 1.0;

        [ObservableProperty]
        private string _detectionMessage = "กำลังรอการตรวจจับ...";

        [ObservableProperty]
        private Brush _detectionBackground = Brushes.Gray;

        [ObservableProperty]
        private BitmapSource? _lastDetectedEmployeeImage;

        [ObservableProperty]
        private AuthenticationLog _lastDetectedEmployee = new();

        [ObservableProperty]
        private string _lastDetectedStatus = "สถานะ: ยังไม่มีข้อมูล";

        [ObservableProperty]
        private DateTime _lastDetectedTime = DateTime.Now;

        [ObservableProperty]
        private bool _isCameraActive = false;

        public DashboardViewModel(ICameraService cameraService, IServiceProvider serviceProvider)
        {
            _cameraService = cameraService;
            _serviceProvider = serviceProvider;

            // ลงทะเบียนรับ event จาก CameraService
            _cameraService.NewFrameAvailable += OnNewFrameAvailable;
        }

        [RelayCommand]
        private async Task ToggleCameraAsync()
        {
            if (IsCameraActive)
            {
                _cameraService.StopCamera();
                IsCameraActive = false;
                DetectionMessage = "กล้องถูกปิด";
            }
            else
            {
                bool success = await _cameraService.StartCameraAsync();
                IsCameraActive = success;
                DetectionMessage = success ? "กำลังรอการตรวจจับ..." : "ไม่สามารถเริ่มกล้องได้";
            }
        }

        [RelayCommand]
        private void ConfigureDetection()
        {
            var settingsDialog = _serviceProvider.GetRequiredService<DetectionSettingsWindow>();
            settingsDialog.Owner = Application.Current.MainWindow;
            bool? result = settingsDialog.ShowDialog();
        }

        [RelayCommand]
        private void ManualRecord()
        {
            MessageBox.Show("ฟังก์ชันการบันทึกด้วยตนเองกำลังอยู่ระหว่างการพัฒนา",
                           "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnNewFrameAvailable(object? sender, BitmapSource frameImage)
        {
            CameraImageSource = frameImage;
            // TODO: เพิ่มการตรวจจับใบหน้าที่นี่ในอนาคต
        }

        public async Task StartCameraAsync()
        {
            DetectionMessage = "กำลังเริ่มกล้อง...";
            bool success = await _cameraService.StartCameraAsync();
            IsCameraActive = success;
            DetectionMessage = success ? "กำลังรอการตรวจจับ..." : "ไม่สามารถเริ่มกล้องได้";
        }

        public void Dispose()
        {
            _cameraService.NewFrameAvailable -= OnNewFrameAvailable;
            _cameraService.StopCamera();
        }
    }
}
