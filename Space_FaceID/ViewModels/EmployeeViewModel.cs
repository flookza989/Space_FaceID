using Space_FaceID.Models.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using Space_FaceID.Repositories;
using Space_FaceID.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Space_FaceID.ViewModels.Dialog;
using Space_FaceID.Views.Controls.Dialogs;
using MaterialDesignThemes.Wpf;
using Space_FaceID.Services.Interfaces;
using Space_FaceID.Models.Enums;

namespace Space_FaceID.ViewModels
{
    public partial class EmployeeViewModel : ObservableObject, IDisposable
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IFaceDataRepository _faceDataRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IImageService _imageService;

        [ObservableProperty]
        private ObservableCollection<User> _employees = [];

        [ObservableProperty]
        private User? _selectedEmployee;

        [ObservableProperty]
        private bool _isEmployeeSelected = false;

        [ObservableProperty]
        private bool _isFaceDataSelected = false;

        [ObservableProperty]
        private UserProfile? _selectedEmployeeProfile;

        [ObservableProperty]
        private ObservableCollection<FaceData> _employeeFaceData = [];

        [ObservableProperty]
        private BitmapImage? _selectedEmployeeImage;

        [ObservableProperty]
        private BitmapImage? _selectedFaceImage;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _loadingMessage = "กำลังโหลดข้อมูล...";

        [ObservableProperty]
        private string? _searchText;

        [ObservableProperty]
        private ObservableCollection<FaceData> _selectedFaceCollection = [];

        [ObservableProperty]
        private FaceData? _selectedFace;

        [ObservableProperty]
        private bool _isAdmin = false;


        public EmployeeViewModel(IUserRepository userRepository,
                                 IUserProfileRepository userProfileRepository,
                                 IFaceDataRepository faceDataRepository,
                                 IServiceProvider serviceProvider,
                                 IImageService imageService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userProfileRepository = userProfileRepository ?? throw new ArgumentNullException(nameof(userProfileRepository));
            _faceDataRepository = faceDataRepository ?? throw new ArgumentNullException(nameof(faceDataRepository));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            LoadingMessage = "กำลังโหลดข้อมูลเริ่มต้น...";
            try
            {
                await LoadEmployeesAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadEmployeesAsync()
        {
            try
            {
                LoadingMessage = "กำลังโหลดข้อมูลพนักงานทั้งหมด...";
                var users = await _userRepository.GetAllUserWithRoleAsync();
                Employees.Clear();

                foreach (var user in users)
                {
                    Employees.Add(user);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการโหลดข้อมูลพนักงาน: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        partial void OnSelectedEmployeeChanged(User? value)
        {
            IsEmployeeSelected = value != null;

            if (SelectedEmployee == null) return;

            IsAdmin = SelectedEmployee.Role != null && SelectedEmployee.Role.Name == RoleName.Admin.ToString();
            LoadEmployeeDetailsAsync().ConfigureAwait(false);
        }

        partial void OnSelectedFaceChanged(FaceData? value)
        {
            IsFaceDataSelected = value != null;

            if (SelectedFace == null) return;
            LoadFaceImage(SelectedFace);
        }

        partial void OnSearchTextChanged(string? value)
        {
            FilterEmployees();
        }

        private async Task LoadEmployeeDetails(int userId)
        {
            IsLoading = true;
            try
            {
                // โหลดข้อมูลโปรไฟล์
                LoadingMessage = "กำลังโหลดข้อมูลของพนักงานที่เลือก...";
                SelectedEmployeeProfile = await _userProfileRepository.GetUserProfileByUserIdAsync(userId);
                // โหลดข้อมูลใบหน้า
                LoadingMessage = "กำลังโหลดข้อมูลใบหน้าของพนักงานที่เลือก...";
                var faceDataList = await _faceDataRepository.GetFaceDatasByUserIdAsync(userId);
                EmployeeFaceData.Clear();

                foreach (var faceData in faceDataList)
                {
                    EmployeeFaceData.Add(faceData);
                }

                // เลือกใบหน้าล่าสุด (ถ้ามี)
                if (EmployeeFaceData.Count > 0)
                {
                    SelectedFace = EmployeeFaceData.OrderByDescending(f => f.CreatedAt).First();
                }

                // โหลดรูปโปรไฟล์ (ถ้ามี)
                if (SelectedEmployeeProfile?.ProfilePicture != null)
                {
                    SelectedEmployeeImage = _imageService.ByteArrayToBitmapImage(SelectedEmployeeProfile.ProfilePicture);
                }
                else
                {
                    SelectedEmployeeImage = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการโหลดข้อมูลพนักงาน: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadFaceImage(FaceData faceData)
        {
            if (faceData?.FaceImage != null)
            {
                SelectedFaceImage = _imageService.ByteArrayToBitmapImage(faceData.FaceImage);
            }
            else
            {
                SelectedFaceImage = null;
            }
        }

        private void FilterEmployees()
        {
            // จะใส่ logic สำหรับการค้นหาพนักงานตาม SearchText
        }

        [RelayCommand]
        private async Task EditEmployeeAsync()
        {
            if (SelectedEmployee == null)
            {
                MessageBox.Show("กรุณาเลือกพนักงานที่ต้องการแก้ไข", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // สร้าง ViewModel สำหรับ Dialog แก้ไขข้อมูล
                var dialogViewModel = _serviceProvider.GetRequiredService<UserEditDialogViewModel>();

                // โหลดข้อมูลพนักงานที่ต้องการแก้ไข
                dialogViewModel.LoadUserData(SelectedEmployee, SelectedEmployeeProfile);

                // สร้าง Dialog View
                var dialogView = _serviceProvider.GetRequiredService<UserEditDialogView>();
                dialogView.DataContext = dialogViewModel;

                // แสดง Dialog
                var result = await DialogHost.Show(dialogView, "RootDialog");

                // ถ้ามีการบันทึกการตั้งค่า (DialogResult == true)
                if (result is bool dialogResult && dialogResult)
                {
                    await LoadEmployeeDetailsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadEmployeeDetailsAsync()
        {
            if (SelectedEmployee == null) return;
            await LoadEmployeeDetails(SelectedEmployee.Id);
        }

        [RelayCommand]
        private async Task DeleteEmployeeAsync()
        {
            if (SelectedEmployee == null)
            {
                MessageBox.Show("กรุณาเลือกพนักงานที่ต้องการลบ", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // ตรวจสอบว่า SelectedEmployee มีข้อมูลบทบาทหรือไม่
                if (SelectedEmployee.Role == null)
                {
                    MessageBox.Show("ไม่สามารถลบพนักงานนี้ได้ เนื่องจากไม่มีข้อมูลบทบาท", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // ตรวจสอบว่าเป็น Admin หรือไม่
                if (SelectedEmployee.Role.Name == RoleName.Admin.ToString())
                {
                    MessageBox.Show("ไม่สามารถลบผู้ดูแลระบบได้ เนื่องจากไม่อนุญาตให้ลบผู้ดูแลระบบ", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // ยืนยันการลบ
                MessageBoxResult result = MessageBox.Show(
                    $"คุณต้องการลบพนักงาน '{SelectedEmployee.Username}' ใช่หรือไม่? การลบข้อมูลนี้ไม่สามารถเรียกคืนได้",
                    "ยืนยันการลบ",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // ลบข้อมูลที่เกี่ยวข้องก่อน (Face Data)
                    IsLoading = true;
                    LoadingMessage = "กำลังลบข้อมูลพนักงาน...";

                    // ลบข้อมูลใบหน้าทั้งหมด
                    var faceDataList = await _faceDataRepository.GetFaceDatasByUserIdAsync(SelectedEmployee.Id);
                    if (faceDataList.Any())
                    {
                        await _faceDataRepository.RemoveRangeAsync(faceDataList);
                    }

                    // ลบข้อมูลโปรไฟล์
                    if (SelectedEmployeeProfile != null)
                    {
                        await _userProfileRepository.RemoveAsync(SelectedEmployeeProfile);
                    }

                    // ลบข้อมูลผู้ใช้
                    await _userRepository.RemoveAsync(SelectedEmployee);

                    // รีเฟรชข้อมูล
                    await LoadEmployeesAsync();

                    // รีเซ็ตการเลือก
                    SelectedEmployee = null;
                    SelectedEmployeeProfile = null;
                    SelectedEmployeeImage = null;
                    EmployeeFaceData.Clear();

                    MessageBox.Show("ลบข้อมูลพนักงานเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการลบข้อมูลพนักงาน: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task AddFaceDataAsync()
        {
            if (SelectedEmployee == null)
            {
                MessageBox.Show("กรุณาเลือกพนักงานที่ต้องการเพิ่มข้อมูลใบหน้า", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // สร้าง ViewModel สำหรับ Dialog เพิ่มข้อมูลใบหน้า
                var dialogViewModel = _serviceProvider.GetRequiredService<FaceDataDialogViewModel>();

                // กำหนดพนักงานที่ต้องการเพิ่มข้อมูลใบหน้า
                dialogViewModel.Initialize(SelectedEmployee);

                // สร้าง Dialog View
                var dialogView = _serviceProvider.GetRequiredService<FaceDataDialogView>();
                dialogView.DataContext = dialogViewModel;

                // แสดง Dialog
                var result = await DialogHost.Show(dialogView, "RootDialog");

                // ถ้ามีการบันทึกการตั้งค่า (DialogResult == true)
                if (result is bool dialogResult && dialogResult)
                {
                    // โหลดข้อมูลใบหน้าใหม่
                    await LoadEmployeeDetailsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteFaceDataAsync()
        {
            if (SelectedFace == null)
            {
                MessageBox.Show("กรุณาเลือกข้อมูลใบหน้าที่ต้องการลบ", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // ยืนยันการลบ
                MessageBoxResult result = MessageBox.Show(
                    $"คุณต้องการลบข้อมูลใบหน้านี้ใช่หรือไม่? การลบข้อมูลนี้ไม่สามารถเรียกคืนได้",
                    "ยืนยันการลบ",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsLoading = true;
                    LoadingMessage = "กำลังลบข้อมูลใบหน้า...";

                    // ลบข้อมูลใบหน้า
                    await _faceDataRepository.RemoveAsync(SelectedFace);

                    // โหลดข้อมูลใหม่
                    await LoadEmployeeDetailsAsync();

                    MessageBox.Show("ลบข้อมูลใบหน้าเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการลบข้อมูลใบหน้า: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadEmployeesAsync();

            if (SelectedEmployee != null)
            {
                await LoadEmployeeDetailsAsync();
            }
        }

        public void Dispose()
        {
            // จะทำความสะอาดทรัพยากรที่ใช้งาน (ถ้ามี)
            GC.SuppressFinalize(this);
        }
    }
}
