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
using Space_FaceID.Services.Implementation;
using Microsoft.Win32;
using System.Diagnostics;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Space_FaceID.Models.DTOs;

namespace Space_FaceID.ViewModels
{
    public partial class EmployeeViewModel(
        IUserService userService,
        IFaceDataService faceDataService,
        IServiceProvider serviceProvider,
        IImageService imageService,
        IExcelService excelService) : ObservableObject, IDisposable
    {
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        private readonly IFaceDataService _faceDataService = faceDataService ?? throw new ArgumentNullException(nameof(faceDataService));
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        private readonly IImageService _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        private readonly IExcelService _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));

        [ObservableProperty]
        private ObservableCollection<User> _employees = [];

        [ObservableProperty]
        private ObservableCollection<User> _filteredEmployees = [];

        [ObservableProperty]
        private User? _selectedEmployee;

        [ObservableProperty]
        private bool _isEmployeeSelected = false;

        [ObservableProperty]
        private bool _isFaceDataSelected = false;

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
                var users = await _userService.GetAllUserWithFullAsync();

                Employees.Clear();
                Employees = [.. users];

                // เซ็ต FilteredEmployees ตั้งต้นให้เป็นข้อมูลเดียวกับ Employees
                FilteredEmployees = [.. Employees];
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
            LoadEmployeeDetails();
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

        private void LoadEmployeeDetails()
        {
            IsLoading = true;
            try
            {
                if (SelectedEmployee == null) return;

                EmployeeFaceData.Clear();

                EmployeeFaceData = [.. SelectedEmployee.FaceDatas];

                // เลือกใบหน้าล่าสุด (ถ้ามี)
                if (EmployeeFaceData.Count > 0)
                {
                    SelectedFace = EmployeeFaceData.OrderByDescending(f => f.CreatedAt).First();
                }

                // โหลดรูปโปรไฟล์ (ถ้ามี)
                if (SelectedEmployee.Profile.ProfilePicture != null)
                {
                    SelectedEmployeeImage = _imageService.ByteArrayToBitmapImage(SelectedEmployee.Profile.ProfilePicture);
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
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // ถ้าไม่มีการค้นหา แสดงพนักงานทั้งหมด
                FilteredEmployees = [.. Employees];
                return;
            }

            // คำค้นหาที่ผู้ใช้กรอก (แปลงเป็นตัวพิมพ์เล็กเพื่อไม่ให้คำนึงถึงตัวพิมพ์ใหญ่-เล็ก)
            string searchQuery = SearchText.ToLower().Trim();

            // กรองพนักงานที่มีข้อมูลตรงกับคำค้นหา
            var filteredList = Employees.Where(emp =>
                // ค้นหาจากชื่อผู้ใช้
                (emp.Username?.ToLower().Contains(searchQuery, StringComparison.CurrentCultureIgnoreCase) == true) ||
                // ค้นหาจากอีเมล
                (emp.Email?.ToLower().Contains(searchQuery, StringComparison.CurrentCultureIgnoreCase) == true) ||
                // ค้นหาจากชื่อ-นามสกุล
                (emp.Profile?.FirstName?.ToLower().Contains(searchQuery, StringComparison.CurrentCultureIgnoreCase) == true) ||
                (emp.Profile?.LastName?.ToLower().Contains(searchQuery, StringComparison.CurrentCultureIgnoreCase) == true) ||
                // ค้นหาจากชื่อเต็ม (ชื่อ + นามสกุล)
                ((emp.Profile?.FirstName + " " + emp.Profile?.LastName)?.ToLower().Contains(searchQuery, StringComparison.CurrentCultureIgnoreCase) == true) ||
                // ค้นหาจากเบอร์โทรศัพท์
                (emp.Profile?.PhoneNumber?.Contains(searchQuery) == true) ||
                // ค้นหาจากบทบาท
                (emp.Role?.Name?.ToLower().Contains(searchQuery, StringComparison.CurrentCultureIgnoreCase) == true)
            ).ToList();

            // อัพเดท FilteredEmployees ด้วยผลลัพธ์การค้นหา
            FilteredEmployees = [.. filteredList];
        }

        private async Task UpdateEmployeeSelected()
        {
            if (SelectedEmployee == null)
                return;

            var updatedUser = await _userService.GetUserWithFullByUserIdAsync(SelectedEmployee.Id);

            if (updatedUser == null)
            {
                MessageBox.Show("ไม่พบข้อมูลพนักงานที่อัพเดต", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var userToFind = Employees.FirstOrDefault(e => e.Id == updatedUser.Id);
            if (userToFind != null)
            {
                var index = Employees.IndexOf(userToFind);
                if (index >= 0)
                {
                    // แทนที่ object เดิมด้วย object ใหม่
                    Employees[index] = updatedUser;
                    SelectedEmployee = updatedUser;
                    LoadEmployeeDetails();
                }
            }
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
                await dialogViewModel.LoadUserData(SelectedEmployee);

                // สร้าง Dialog View
                var dialogView = _serviceProvider.GetRequiredService<UserEditDialogView>();
                dialogView.DataContext = dialogViewModel;

                // แสดง Dialog
                var result = await DialogHost.Show(dialogView, "RootDialog");

                // ถ้ามีการบันทึกการตั้งค่า (DialogResult == true)
                if (result is bool dialogResult && dialogResult)
                {
                    await UpdateEmployeeSelected();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                    IsLoading = true;
                    LoadingMessage = "กำลังลบข้อมูลพนักงาน...";

                    // ลบข้อมูลผู้ใช้
                    await _userService.RemoveAsync(SelectedEmployee);

                    // รีเฟรชข้อมูล
                    await LoadEmployeesAsync();

                    // รีเซ็ตการเลือก
                    SelectedEmployee = null;
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
                    await UpdateEmployeeSelected();
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
                    await _faceDataService.RemoveAsync(SelectedFace);

                    // โหลดข้อมูลใหม่
                    LoadEmployeeDetails();

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
                LoadEmployeeDetails();
            }
        }

        [RelayCommand]
        private async Task AddEmployeeAsync()
        {
            try
            {
                // สร้าง ViewModel สำหรับ Dialog เพิ่มพนักงาน
                var dialogViewModel = _serviceProvider.GetRequiredService<UserEditDialogViewModel>();

                // เตรียมข้อมูลสำหรับการเพิ่มพนักงานใหม่
                await dialogViewModel.InitializeNewUser();

                // สร้าง Dialog View
                var dialogView = _serviceProvider.GetRequiredService<UserEditDialogView>();
                dialogView.DataContext = dialogViewModel;

                // แสดง Dialog
                var result = await DialogHost.Show(dialogView, "RootDialog");

                // ถ้ามีการบันทึกการตั้งค่า (DialogResult == true)
                if (result is bool dialogResult && dialogResult)
                {
                    // รีเฟรชข้อมูลหลังจากเพิ่มพนักงานใหม่
                    await LoadEmployeesAsync();

                    // เลือกพนักงานที่เพิ่มใหม่ล่าสุด (ถ้ามี)
                    if (dialogViewModel.UserId > 0 && Employees.Any())
                    {
                        var newUser = Employees.FirstOrDefault(e => e.Id == dialogViewModel.UserId);
                        if (newUser != null)
                        {
                            SelectedEmployee = newUser;
                        }
                    }

                    MessageBox.Show("เพิ่มพนักงานใหม่เรียบร้อยแล้ว", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ImportFromExcelAsync()
        {
            try
            {
                // เปิด dialog เลือกไฟล์ Excel
                OpenFileDialog openFileDialog = new()
                {
                    Title = "เลือกไฟล์ Excel สำหรับนำเข้าพนักงาน",
                    Filter = "Excel Files|*.xlsx;*.xls",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() != true)
                {
                    return; // ผู้ใช้ยกเลิกการเลือกไฟล์
                }

                // แสดง dialog ยืนยันการนำเข้า
                var confirmResult = MessageBox.Show(
                    "คุณแน่ใจหรือไม่ที่จะนำเข้าข้อมูลพนักงานจากไฟล์ Excel นี้?",
                    "ยืนยันการนำเข้าข้อมูล",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmResult != MessageBoxResult.Yes)
                {
                    return;
                }

                IsLoading = true;
                LoadingMessage = "กำลังนำเข้าข้อมูลพนักงานจาก Excel...";

                // นำเข้าข้อมูลจากไฟล์ Excel
                ImportResult result = await _excelService.ImportEmployeesFromExcelAsync(openFileDialog.FileName);
                
                // รีเฟรชข้อมูลพนักงานหลังการนำเข้า
                await LoadEmployeesAsync();
                
                // สร้างข้อความสรุปผลการนำเข้า
                StringBuilder summaryBuilder = new();
                summaryBuilder.AppendLine($"สรุปผลการนำเข้าข้อมูลพนักงาน:\n");
                summaryBuilder.AppendLine($"จำนวนแถวทั้งหมด: {result.ActualRows} แถว");
                summaryBuilder.AppendLine($"นำเข้าสำเร็จ: {result.SuccessCount} รายการ");
                summaryBuilder.AppendLine($"เกิดข้อผิดพลาด: {result.ErrorCount} รายการ");
                summaryBuilder.AppendLine($"เวลาที่ใช้: {result.TotalTimeSeconds:F2} วินาที\n");
                
                // ถ้ามีข้อผิดพลาด แสดงรายละเอียด
                if (result.ErrorCount > 0)
                {
                    summaryBuilder.AppendLine("รายละเอียดข้อผิดพลาด:");
                    foreach (var error in result.Errors)
                    {
                        string usernameInfo = !string.IsNullOrEmpty(error.Username) ? $" (Username: {error.Username})" : "";
                        summaryBuilder.AppendLine($"- แถวที่ {error.RowNumber}{usernameInfo}: {error.ErrorMessage}");
                    }
                }

                // แสดงผลสรุปการนำเข้า
                MessageBox.Show(
                    summaryBuilder.ToString(),
                    result.ErrorCount > 0 ? "นำเข้าข้อมูลเสร็จสิ้น (มีข้อผิดพลาด)" : "นำเข้าข้อมูลเสร็จสิ้น",
                    MessageBoxButton.OK,
                    result.ErrorCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการนำเข้าข้อมูลจาก Excel: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task DownloadTemplateAsync()
        {
            try
            {
                // เปิด dialog เลือกตำแหน่งที่จะบันทึกไฟล์
                SaveFileDialog saveFileDialog = new()
                {
                    Title = "บันทึกเทมเพลต Excel สำหรับนำเข้าพนักงาน",
                    Filter = "Excel Files|*.xlsx",
                    FileName = "Employee_Import_Template.xlsx",
                    DefaultExt = ".xlsx"
                };

                if (saveFileDialog.ShowDialog() != true)
                {
                    return; // ผู้ใช้ยกเลิกการบันทึกไฟล์
                }

                IsLoading = true;
                LoadingMessage = "กำลังสร้างเทมเพลต Excel...";

                // สร้างไฟล์ Excel
                await _excelService.CreateEmployeeTemplateAsync(saveFileDialog.FileName);

                // แสดงข้อความว่าสร้างไฟล์สำเร็จ
                MessageBox.Show(
                    $"สร้างเทมเพลต Excel สำหรับนำเข้าข้อมูลพนักงานเรียบร้อยแล้ว\n\nไฟล์ถูกบันทึกที่:\n{saveFileDialog.FileName}",
                    "สำเร็จ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // ถามผู้ใช้ว่าต้องการเปิดไฟล์หรือไม่
                var openFileResult = MessageBox.Show(
                    "คุณต้องการเปิดตำแหน่งไฟล์หรือไม่?",
                    "เปิดตำแหน่งไฟล์",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (openFileResult == MessageBoxResult.Yes)
                {
                    // เปิด Explorer ไปที่โฟลเดอร์ที่บันทึกไฟล์
                    Process.Start("explorer.exe", $"/select,\"{saveFileDialog.FileName}\"");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการสร้างเทมเพลต Excel: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void Dispose()
        {
            // จะทำความสะอาดทรัพยากรที่ใช้งาน (ถ้ามี)
            GC.SuppressFinalize(this);
        }
    }
}
