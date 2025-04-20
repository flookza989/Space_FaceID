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

namespace Space_FaceID.ViewModels
{
    public partial class EmployeeViewModel : ObservableObject, IDisposable
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IFaceDataRepository _faceDataRepository;
        private readonly IServiceProvider _serviceProvider;

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


        public EmployeeViewModel(IUserRepository userRepository,
                                 IUserProfileRepository userProfileRepository,
                                 IFaceDataRepository faceDataRepository,
                                 IServiceProvider serviceProvider)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userProfileRepository = userProfileRepository ?? throw new ArgumentNullException(nameof(userProfileRepository));
            _faceDataRepository = faceDataRepository ?? throw new ArgumentNullException(nameof(faceDataRepository));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
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
                var users = await _userRepository.GetAllAsync();
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

            LoadEmployeeDetails(SelectedEmployee.Id);
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

        private async void LoadEmployeeDetails(int userId)
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
                    SelectedEmployeeImage = ByteArrayToBitmapImage(SelectedEmployeeProfile.ProfilePicture);
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
                SelectedFaceImage = ByteArrayToBitmapImage(faceData.FaceImage);
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
                var dialogResult = await DialogHost.Show(dialogView, "RootDialog");

                // ถ้ามีการเปลี่ยนแปลงข้อมูล ให้โหลดข้อมูลใหม่
                if (dialogViewModel.HasChanges)
                {
                    await ReloadEmployeeDetails();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ReloadEmployeeDetails()
        {
            if (SelectedEmployee == null) return;

            int userId = SelectedEmployee.Id;
            // โหลดข้อมูลผู้ใช้ใหม่
            SelectedEmployee = await _userRepository.GetByIdAsync(userId);

            // โหลดข้อมูลรายละเอียดใหม่
            LoadEmployeeDetails(userId);
        }

        [RelayCommand]
        private void DeleteEmployee()
        {
            // จะใส่ logic สำหรับการลบข้อมูลพนักงาน
            MessageBox.Show("ฟังก์ชันการลบข้อมูลพนักงานยังไม่เปิดให้ใช้งาน", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void AddFaceData()
        {
            // จะใส่ logic สำหรับการเพิ่มข้อมูลใบหน้า
            MessageBox.Show("ฟังก์ชันการเพิ่มข้อมูลใบหน้ายังไม่เปิดให้ใช้งาน", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void DeleteFaceData()
        {
            // จะใส่ logic สำหรับการลบข้อมูลใบหน้า
            MessageBox.Show("ฟังก์ชันการลบข้อมูลใบหน้ายังไม่เปิดให้ใช้งาน", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadEmployeesAsync();

            if (SelectedEmployee != null)
            {
                await ReloadEmployeeDetails();
            }
        }

        private BitmapImage? ByteArrayToBitmapImage(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0) return null;

            try
            {
                using MemoryStream stream = new(byteArray);
                BitmapImage image = new();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze(); // ทำให้ thread-safe
                return image;
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            // จะทำความสะอาดทรัพยากรที่ใช้งาน (ถ้ามี)
            GC.SuppressFinalize(this);
        }
    }
}
