using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Space_FaceID.Models.Entities;
using Space_FaceID.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.ComponentModel;
using Space_FaceID.Services.Interfaces;
using Space_FaceID.Repositories.Interfaces;
using MaterialDesignThemes.Wpf;

namespace Space_FaceID.ViewModels.Dialog
{
    public partial class UserEditDialogViewModel(
        IUserService userService,
        IImageService imageService) : ObservableObject
    {
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        private readonly IImageService _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));

        private string? _originalFirstName;
        private string? _originalLastName;
        private string? _originalEmail;
        private string? _originalPhoneNumber;
        private DateTime? _originalDateOfBirth;
        private string? _originalGender;
        private string? _originalAddress;
        private bool? _originalIsActive;
        private byte[]? _originalProfilePicture;


        [ObservableProperty]
        private User? _user;

        [ObservableProperty]
        private string? _firstName;

        [ObservableProperty]
        private string? _lastName;

        [ObservableProperty]
        private string? _email;

        [ObservableProperty]
        private string? _phoneNumber;

        [ObservableProperty]
        private DateTime? _dateOfBirth;

        [ObservableProperty]
        private string? _gender;

        [ObservableProperty]
        private string? _address;

        [ObservableProperty]
        private bool? _isActive;

        [ObservableProperty]
        private byte[]? _profilePicture;

        [ObservableProperty]
        private BitmapImage? _profileImage;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _loadingMessage = string.Empty;

        [ObservableProperty]
        private bool _isDataChanged = false;

        [ObservableProperty]
        private bool _isHasImage = false;

        public List<string> GenderOptions { get; } = ["ชาย", "หญิง", "อื่นๆ"];

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            CheckForChanges();
        }

        partial void OnProfileImageChanged(BitmapImage? value) => IsHasImage = value != null;

        public void LoadUserData(User user)
        {
            User = user;

            // กำหนดค่าเริ่มต้นจากข้อมูลผู้ใช้
            Email = user.Email;
            IsActive = user.IsActive;

            // กำหนดค่าเริ่มต้นจากข้อมูลโปรไฟล์
            FirstName = user.Profile.FirstName;
            LastName = user.Profile.LastName;
            PhoneNumber = user.Profile.PhoneNumber;
            DateOfBirth = user.Profile.DateOfBirth;
            Gender = user.Profile.Gender;
            Address = user.Profile.Address;
            ProfilePicture = user.Profile.ProfilePicture;

            // แสดงรูปโปรไฟล์
            if (ProfilePicture != null)
            {
                ProfileImage = _imageService.ByteArrayToBitmapImage(ProfilePicture);
            }

            // เก็บค่าเริ่มต้นเพื่อตรวจสอบการเปลี่ยนแปลง
            _originalFirstName = FirstName;
            _originalLastName = LastName;
            _originalEmail = Email;
            _originalPhoneNumber = PhoneNumber;
            _originalDateOfBirth = DateOfBirth;
            _originalGender = Gender;
            _originalAddress = Address;
            _originalIsActive = IsActive;
            _originalProfilePicture = ProfilePicture;



            PropertyChanged += OnPropertyChanged;
        }

        [RelayCommand]
        private void BrowseImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "เลือกรูปโปรไฟล์",
                Filter = "รูปภาพ|*.jpg;*.jpeg;*.png;*.bmp",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // อ่านไฟล์รูปภาพ
                    byte[] imageData = File.ReadAllBytes(openFileDialog.FileName);

                    // กำหนดค่า ProfilePicture
                    ProfilePicture = imageData;

                    // แสดงรูปภาพ
                    ProfileImage = _imageService.ByteArrayToBitmapImage(ProfilePicture);

                    // ตรวจสอบการเปลี่ยนแปลงหลังจากเลือกรูปภาพ
                    CheckForChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ไม่สามารถอ่านไฟล์รูปภาพได้: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void RemoveImage()
        {
            ProfilePicture = null;
            ProfileImage = null;

            CheckForChanges();
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (User == null) return;

            IsLoading = true;
            LoadingMessage = "กำลังบันทึกข้อมูล...";
            try
            {
                // อัพเดทข้อมูลผู้ใช้
                User.Email = Email;
                User.IsActive = IsActive ?? false;

                // อัพเดทข้อมูลโปรไฟล์
                User.Profile.FirstName = FirstName;
                User.Profile.LastName = LastName;
                User.Profile.PhoneNumber = PhoneNumber;
                User.Profile.DateOfBirth = DateOfBirth;
                User.Profile.Gender = Gender;
                User.Profile.Address = Address;
                User.Profile.ProfilePicture = ProfilePicture;

                // บันทึกข้อมูลผู้ใช้
                var RowAffectedUpdateUser = await _userService.UpdateAsync(User);

                MessageBox.Show("บันทึกข้อมูลเรียบร้อย", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);

                // ปิด Dialog โดยส่งค่า true กลับ (บันทึกสำเร็จ)
                DialogHost.Close("RootDialog", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการบันทึกข้อมูล: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CheckForChanges()
        {
            IsDataChanged =
                FirstName != _originalFirstName ||
                LastName != _originalLastName ||
                Email != _originalEmail ||
                PhoneNumber != _originalPhoneNumber ||
                DateOfBirth != _originalDateOfBirth ||
                Gender != _originalGender ||
                Address != _originalAddress ||
                IsActive != _originalIsActive ||
                ProfilePicture != _originalProfilePicture;
        }
    }
}
