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

namespace Space_FaceID.ViewModels.Dialog
{
    public partial class UserEditDialogViewModel : ObservableObject
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserProfileRepository _userProfileRepository;

        [ObservableProperty]
        private User? _user;

        [ObservableProperty]
        private UserProfile? _userProfile;

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
        private bool _isActive;

        [ObservableProperty]
        private byte[]? _profilePicture;

        [ObservableProperty]
        private BitmapImage? _profileImage;

        [ObservableProperty]
        private bool _isSaving = false;

        [ObservableProperty]
        private string _dialogTitle = "แก้ไขข้อมูลพนักงาน";

        public bool HasChanges { get; private set; } = false;

        public List<string> GenderOptions { get; } = new List<string> { "ชาย", "หญิง", "อื่นๆ" };

        public UserEditDialogViewModel(IUserRepository userRepository, IUserProfileRepository userProfileRepository)
        {
            _userRepository = userRepository;
            _userProfileRepository = userProfileRepository;
        }

        public void LoadUserData(User user, UserProfile? userProfile)
        {
            User = user;
            UserProfile = userProfile ?? new UserProfile { UserId = user.Id };

            // กำหนดค่าเริ่มต้นจากข้อมูลผู้ใช้
            Email = user.Email;
            IsActive = user.IsActive;

            // กำหนดค่าเริ่มต้นจากข้อมูลโปรไฟล์
            FirstName = userProfile?.FirstName;
            LastName = userProfile?.LastName;
            PhoneNumber = userProfile?.PhoneNumber;
            DateOfBirth = userProfile?.DateOfBirth;
            Gender = userProfile?.Gender;
            Address = userProfile?.Address;
            ProfilePicture = userProfile?.ProfilePicture;

            // แสดงรูปโปรไฟล์
            if (ProfilePicture != null)
            {
                ProfileImage = ByteArrayToBitmapImage(ProfilePicture);
            }

            HasChanges = false;
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
                    ProfileImage = ByteArrayToBitmapImage(ProfilePicture);
                    
                    HasChanges = true;
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
            HasChanges = true;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (User == null || UserProfile == null) return;

            IsSaving = true;
            try
            {
                // อัพเดทข้อมูลผู้ใช้
                User.Email = Email;
                User.IsActive = IsActive;

                // อัพเดทข้อมูลโปรไฟล์
                UserProfile.FirstName = FirstName;
                UserProfile.LastName = LastName;
                UserProfile.PhoneNumber = PhoneNumber;
                UserProfile.DateOfBirth = DateOfBirth;
                UserProfile.Gender = Gender;
                UserProfile.Address = Address;
                UserProfile.ProfilePicture = ProfilePicture;

                // บันทึกข้อมูลผู้ใช้
                await _userRepository.UpdateAsync(User);

                // บันทึกข้อมูลโปรไฟล์
                if (UserProfile.Id == 0) // กรณีเป็นโปรไฟล์ใหม่
                {
                    await _userProfileRepository.AddAsync(UserProfile);
                }
                else
                {
                    await _userProfileRepository.UpdateAsync(UserProfile);
                }

                HasChanges = false;
                MessageBox.Show("บันทึกข้อมูลเรียบร้อย", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการบันทึกข้อมูล: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private BitmapImage? ByteArrayToBitmapImage(byte[]? byteArray)
        {
            if (byteArray == null || byteArray.Length == 0) return null;

            try
            {
                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze(); // ทำให้ thread-safe
                    return image;
                }
            }
            catch
            {
                return null;
            }
        }

        // ตรวจจับการเปลี่ยนแปลงข้อมูล
        partial void OnFirstNameChanged(string? value) => HasChanges = true;
        partial void OnLastNameChanged(string? value) => HasChanges = true;
        partial void OnEmailChanged(string? value) => HasChanges = true;
        partial void OnPhoneNumberChanged(string? value) => HasChanges = true;
        partial void OnDateOfBirthChanged(DateTime? value) => HasChanges = true;
        partial void OnGenderChanged(string? value) => HasChanges = true;
        partial void OnAddressChanged(string? value) => HasChanges = true;
        partial void OnIsActiveChanged(bool value) => HasChanges = true;
    }
}
