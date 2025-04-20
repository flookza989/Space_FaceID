using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Space_FaceID.Helpers.Extensions;
using Space_FaceID.Models.Entities;
using Space_FaceID.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace Space_FaceID.ViewModels.Dialog
{
    public partial class DetectionSettingsDialogViewModel : ObservableObject, IDisposable
    {
        private readonly ICameraService _cameraService;
        private readonly ICameraSettingService _cameraSettingService;
        private readonly IFaceDetectionSettingService _faceDetectionSettingService;
        private readonly IFaceRecognizeSettingService _faceRecognizeSettingService;

        private static readonly int _defaultCamera = 0;
        private static readonly int _defaultFps = 24;
        private static readonly int _defaultFaceSize = 20;
        private static readonly int _defaultDetectionThreshold = 90;
        private static readonly int _defaultMaxWidth = 2000;
        private static readonly int _defaultMaxHeight = 2000;
        private static readonly bool _defaultIsFaceDetectionEnabled = true;
        private static readonly string _defaultResolution = "640x480";
        private static readonly string _defaultFaceType = "Normal";

        private int _originalCameraIndex = _defaultCamera;
        private int _originalFpsValue = _defaultFps;
        private string _originalResolution = _defaultResolution;
        private bool _originalIsFaceDetectionEnabled = _defaultIsFaceDetectionEnabled;
        private int _originalFaceSizeValue = _defaultFaceSize;
        private int _originalDetectionThresholdValue = _defaultDetectionThreshold;
        private int _originalMaxWidthValue = _defaultMaxWidth;
        private int _originalMaxHeightValue = _defaultMaxHeight;
        private string _originalFaceType = _defaultFaceType;

        [ObservableProperty]
        private bool _isDataChanged = false;

        [ObservableProperty]
        private ObservableCollection<int> _cameras = [];

        [ObservableProperty]
        private int _selectedCamera = _defaultCamera;

        [ObservableProperty]
        private ObservableCollection<string> _resolutions =
        [
            "320x240",
            "640x480",
            "1280x720",
            "1920x1080"
        ];

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _loadingMessage = "กำลังโหลดข้อมูล...";

        [ObservableProperty]
        private string _selectedResolution = _defaultResolution;

        [ObservableProperty]
        private int _fpsValue = _defaultFps;

        [ObservableProperty]
        private bool _isFaceDetectionEnabled;

        [ObservableProperty]
        private int _faceSizeValue = _defaultFaceSize;

        [ObservableProperty]
        private int _detectionThresholdValue = _defaultDetectionThreshold;

        [ObservableProperty]
        private int _maxWidthValue = _defaultMaxWidth;

        [ObservableProperty]
        private int _maxHeightValue = _defaultMaxHeight;

        [ObservableProperty]
        private ObservableCollection<string> _faceTypes = [];

        [ObservableProperty]
        private string _selectedFaceType = _defaultFaceType;

        private CameraSetting? _cameraSetting;
        private FaceDetectionSetting? _faceDetectionSetting;

        public DetectionSettingsDialogViewModel(
            ICameraService cameraService,
            ICameraSettingService cameraSettingService,
            IFaceDetectionSettingService faceDetectionSettingService,
            IFaceRecognizeSettingService faceRecognizeSettingService)
        {
            _cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
            _cameraSettingService = cameraSettingService ?? throw new ArgumentNullException(nameof(cameraSettingService));
            _faceDetectionSettingService = faceDetectionSettingService ?? throw new ArgumentNullException(nameof(faceDetectionSettingService));
            _faceRecognizeSettingService = faceRecognizeSettingService ?? throw new ArgumentNullException(nameof(faceRecognizeSettingService));
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            CheckForChanges();
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            LoadingMessage = "กำลังโหลดข้อมูล...";
            try
            {
                await LoadCamerasAsync();
                await LoadCameraSettingAsync();
                await LoadFaceDetectionSettingAsync();
                await LoadFaceRecognizeSettingAsync();
                PropertyChanged += OnPropertyChanged;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCamerasAsync()
        {
            var cameras = await _cameraService.FindConnectedCamerasAsync();
            Cameras.Clear();
            foreach (var camera in cameras)
            {
                Cameras.Add(camera);
            }
            if (Cameras.Count > 0)
            {
                SelectedCamera = Cameras[0];
            }
        }

        private async Task LoadCameraSettingAsync()
        {
            try
            {
                _cameraSetting = await _cameraSettingService.GetActiveCameraSettingAsync();

                if (_cameraSetting == null)
                {
                    // สร้างการตั้งค่าใหม่หากไม่มีข้อมูล
                    _cameraSetting = new CameraSetting
                    {
                        CameraIndex = _defaultCamera,
                        FrameRate = _defaultFps,
                        FrameWidth = 640,
                        FrameHeight = 480,
                        UpdatedBy = "System",
                        LastUpdated = DateTime.Now
                    };
                    await _cameraSettingService.AddAsync(_cameraSetting);
                }

                SelectedCamera = _cameraSetting.CameraIndex;
                FpsValue = _cameraSetting.FrameRate;
                SelectedResolution = ResolutionExtensions.ToResolutionString((_cameraSetting.FrameWidth, _cameraSetting.FrameHeight));

                _originalCameraIndex = SelectedCamera;
                _originalFpsValue = FpsValue;
                _originalResolution = SelectedResolution;
            }
            catch (Exception ex)
            {
                // ใช้ค่าเริ่มต้นหากเกิดข้อผิดพลาด
                SelectedCamera = _defaultCamera;
                FpsValue = _defaultFps;
                SelectedResolution = _defaultResolution;

                MessageBox.Show($"เกิดข้อผิดพลาดในการโหลดข้อมูลการตั้งค่ากล้อง: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadFaceDetectionSettingAsync()
        {
            try
            {
                _faceDetectionSetting = await _faceDetectionSettingService.GetActiveFaceDetectionSettingAsync();

                if (_faceDetectionSetting == null)
                {
                    // สร้างการตั้งค่าใหม่หากไม่มีข้อมูล
                    _faceDetectionSetting = new FaceDetectionSetting
                    {
                        IsEnabled = _defaultIsFaceDetectionEnabled,
                        FaceSize = _defaultFaceSize,
                        DetectionThreshold = _defaultDetectionThreshold / 100.0f,
                        MaxWidth = _defaultMaxWidth,
                        MaxHeight = _defaultMaxHeight,
                        UpdatedBy = "System",
                        LastUpdated = DateTime.Now
                    };
                    await _faceDetectionSettingService.AddAsync(_faceDetectionSetting);
                }

                IsFaceDetectionEnabled = _faceDetectionSetting.IsEnabled;
                FaceSizeValue = _faceDetectionSetting.FaceSize;
                DetectionThresholdValue = (int)(_faceDetectionSetting.DetectionThreshold * 100);
                MaxWidthValue = _faceDetectionSetting.MaxWidth;
                MaxHeightValue = _faceDetectionSetting.MaxHeight;

                _originalIsFaceDetectionEnabled = IsFaceDetectionEnabled;
                _originalFaceSizeValue = FaceSizeValue;
                _originalDetectionThresholdValue = DetectionThresholdValue;
                _originalMaxWidthValue = MaxWidthValue;
                _originalMaxHeightValue = MaxHeightValue;
            }
            catch (Exception ex)
            {
                // ใช้ค่าเริ่มต้นหากเกิดข้อผิดพลาด
                IsFaceDetectionEnabled = _defaultIsFaceDetectionEnabled;
                FaceSizeValue = _defaultFaceSize;
                DetectionThresholdValue = _defaultDetectionThreshold;
                MaxWidthValue = _defaultMaxWidth;
                MaxHeightValue = _defaultMaxHeight;

                MessageBox.Show($"เกิดข้อผิดพลาดในการโหลดข้อมูลการตั้งค่าการตรวจจับใบหน้า: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadFaceRecognizeSettingAsync()
        {
            try
            {
                var faceRecognizeSettings = await _faceRecognizeSettingService.GetAllAsync();
                if (faceRecognizeSettings.Any())
                {
                    FaceTypes.Clear();
                    foreach (var faceType in faceRecognizeSettings)
                    {
                        FaceTypes.Add(faceType.Name);

                        if (faceType.IsEnabled)
                        {
                            SelectedFaceType = faceType.Name;
                        }
                    }

                    _originalFaceType = SelectedFaceType;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการโหลดข้อมูลการตั้งค่าการจดจำใบหน้า: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckForChanges()
        {
            IsDataChanged =
                SelectedCamera != _originalCameraIndex ||
                FpsValue != _originalFpsValue ||
                SelectedResolution != _originalResolution ||
                IsFaceDetectionEnabled != _originalIsFaceDetectionEnabled ||
                FaceSizeValue != _originalFaceSizeValue ||
                DetectionThresholdValue != _originalDetectionThresholdValue ||
                MaxWidthValue != _originalMaxWidthValue ||
                MaxHeightValue != _originalMaxHeightValue ||
                SelectedFaceType != _originalFaceType;
        }


        [RelayCommand]
        public async Task SaveAsync()
        {
            IsLoading = true;
            LoadingMessage = "กำลังบันทึกข้อมูล...";
            try
            {
                if (_cameraSetting != null)
                {
                    _cameraSetting.CameraIndex = SelectedCamera;
                    _cameraSetting.FrameRate = FpsValue;
                    var (width, height) = SelectedResolution.ToFrameDimensions(); // ใช้ named tuple
                    _cameraSetting.FrameWidth = width;
                    _cameraSetting.FrameHeight = height;
                    _cameraSetting.LastUpdated = DateTime.Now;
                    await _cameraSettingService.UpdateAsync(_cameraSetting);
                }

                if (_faceDetectionSetting != null)
                {
                    _faceDetectionSetting.IsEnabled = IsFaceDetectionEnabled;
                    _faceDetectionSetting.FaceSize = FaceSizeValue;
                    _faceDetectionSetting.DetectionThreshold = DetectionThresholdValue / 100.0f;
                    _faceDetectionSetting.MaxWidth = MaxWidthValue;
                    _faceDetectionSetting.MaxHeight = MaxHeightValue;
                    _faceDetectionSetting.LastUpdated = DateTime.Now;
                    await _faceDetectionSettingService.UpdateAsync(_faceDetectionSetting);
                }

                if (SelectedFaceType != _originalFaceType)
                {
                    var faceRecognizeSettings = await _faceRecognizeSettingService.GetAllAsync();
                    foreach (var faceType in faceRecognizeSettings)
                    {
                        faceType.IsEnabled = faceType.Name == SelectedFaceType;
                    }
                    await _faceRecognizeSettingService.UpdateRangeAsync(faceRecognizeSettings);
                }

                // แสดงข้อความยืนยันการบันทึก
                MessageBox.Show("บันทึกการตั้งค่าเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);

                // ปิด Dialog โดยส่งค่า true กลับ (บันทึกสำเร็จ)
                DialogHost.Close("RootDialog", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการบันทึก: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void Reset()
        {
            // ถามยืนยันก่อนรีเซ็ต
            var result = MessageBox.Show(
                "คุณแน่ใจหรือไม่ที่จะรีเซ็ตการตั้งค่าทั้งหมดเป็นค่าเริ่มต้น?",
                "ยืนยันการรีเซ็ต",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            SelectedCamera = _defaultCamera;
            FpsValue = _defaultFps;
            SelectedResolution = _defaultResolution;
            IsFaceDetectionEnabled = _defaultIsFaceDetectionEnabled;
            FaceSizeValue = _defaultFaceSize;
            DetectionThresholdValue = _defaultDetectionThreshold;
            MaxWidthValue = _defaultMaxWidth;
            MaxHeightValue = _defaultMaxHeight;

            MessageBox.Show("รีเซ็ตการตั้งค่าเป็นค่าเริ่มต้นแล้ว", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void Dispose()
        {
            Cameras.Clear();

            GC.SuppressFinalize(this);
        }
    }
}
