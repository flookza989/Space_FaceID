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
using System.Windows.Input;

namespace Space_FaceID.ViewModels.Dialog
{
    public partial class UserEditDialogViewModel(IUserService userService, IImageService imageService, IRoleService roleService) : ObservableObject
    {
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        private readonly IImageService _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        private readonly IRoleService _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));

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
        private bool _isFormValid = false;

        [ObservableProperty]
        private bool _canSave = false;

        [ObservableProperty]
        private string? _newPassword;

        [ObservableProperty]
        private string? _confirmPassword;

        [ObservableProperty]
        private bool _isChangePassword = false;

        [ObservableProperty]
        private string? _passwordErrorMessage;

        [ObservableProperty]
        private bool _isHasImage = false;

        [ObservableProperty]
        private int _userId;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private bool _isNewUser = false;

        [ObservableProperty]
        private List<Role> _availableRoles = [];

        [ObservableProperty]
        private Role? _selectedRole;
        
        [ObservableProperty]
        private bool _canChangeRole = true; // บ่งชี้ว่าสามารถเปลี่ยน Role ได้หรือไม่
        
        [ObservableProperty]
        private bool _isUserAdmin = false; // บ่งชี้ว่าผู้ใช้ที่กำลังแก้ไขเป็น Admin หรือไม่

        public List<string> GenderOptions { get; } = ["ชาย", "หญิง", "อื่นๆ"];

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            CheckForChanges();
        }

        partial void OnProfileImageChanged(BitmapImage? value) => IsHasImage = value != null;

        public async Task LoadUserData(User user)
        {
            User = user;
            UserId = user.Id;
            Username = user.Username;
            IsNewUser = false;
            
            // ตรวจสอบว่าผู้ใช้เป็น Admin หรือไม่
            IsUserAdmin = user.Role.Name == "Admin";

            // กำหนดค่าเริ่มต้นจากข้อมูลผู้ใช้
            Email = user.Email;
            IsActive = user.IsActive;
            
            // โหลดบทบาทที่สามารถเลือกได้ (ยกเว้น Admin สำหรับการแก้ไข)
            var roles = await _roleService.GetRolesWithoutAdminAsync();
            AvailableRoles = [.. roles];
            
            // ตั้งค่าบทบาทปัจจุบัน
            if (IsUserAdmin)
            {
                // ถ้าเป็น Admin ให้เพิ่มในรายการบทบาทที่สามารถเลือกได้
                AvailableRoles.Add(user.Role);
                SelectedRole = user.Role;
                CanChangeRole = false; // ไม่สามารถเปลี่ยนบทบาทของ Admin ได้
            }
            else
            {
                // ถ้าไม่ใช่ Admin ให้เลือกบทบาทปัจจุบัน
                SelectedRole = AvailableRoles.FirstOrDefault(r => r.Id == user.RoleId);
                CanChangeRole = true; // สามารถเปลี่ยนบทบาทได้
            }

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
            
            // กำหนดค่าเริ่มต้นของ CanSave
            CanSave = false;
        }

        public async Task InitializeNewUser()
        {
            try
            {
                IsLoading = true;
                LoadingMessage = "กำลังโหลดข้อมูล...";

                IsNewUser = true;
                IsChangePassword = true; // สำหรับผู้ใช้ใหม่ต้องตั้งรหัสผ่าน

                // ตั้งค่าเริ่มต้น
                UserId = 0;
                Username = string.Empty;
                Email = string.Empty;
                FirstName = string.Empty;
                LastName = string.Empty;
                PhoneNumber = string.Empty;
                DateOfBirth = DateTime.Now.AddYears(-20); // อายุเริ่มต้น 20 ปี
                Gender = "ชาย"; // เพศเริ่มต้น
                Address = string.Empty;
                IsActive = true; // เปิดใช้งานเริ่มต้น
                ProfilePicture = null;
                ProfileImage = null;

                // โหลดบทบาทยกเว้น Admin
                var roles = await _roleService.GetRolesWithoutAdminAsync();
                AvailableRoles = [.. roles];

                // เลือกบทบาทเริ่มต้นเป็น User
                SelectedRole = AvailableRoles.FirstOrDefault(r => r.Name == "User");

                // เก็บค่าเริ่มต้น
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
                
                // กำหนดค่าเริ่มต้นของ CanSave
                CanSave = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการเริ่มต้น: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
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
            try
            {
                IsLoading = true;
                LoadingMessage = "กำลังบันทึกข้อมูล...";

                // ตรวจสอบรหัสผ่าน
                if (IsChangePassword || IsNewUser)
                {
                    if (string.IsNullOrEmpty(NewPassword))
                    {
                        MessageBox.Show("กรุณากรอกรหัสผ่าน", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                        IsLoading = false;
                        return;
                    }

                    if (NewPassword != ConfirmPassword)
                    {
                        MessageBox.Show("รหัสผ่านไม่ตรงกัน กรุณาตรวจสอบอีกครั้ง", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                        IsLoading = false;
                        return;
                    }
                }

                // สำหรับผู้ใช้ใหม่
                if (IsNewUser)
                {
                    if (string.IsNullOrEmpty(Username))
                    {
                        MessageBox.Show("กรุณากรอกชื่อผู้ใช้", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                        IsLoading = false;
                        return;
                    }

                    if (SelectedRole == null)
                    {
                        MessageBox.Show("กรุณาเลือกบทบาทผู้ใช้", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                        IsLoading = false;
                        return;
                    }

                    if (NewPassword == null)
                    {
                        MessageBox.Show("กรุณากรอกรหัสผ่านสำหรับผู้ใช้ใหม่", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                        IsLoading = false;
                        return;
                    }

                    // สร้างข้อมูลผู้ใช้ใหม่
                    var newUser = new User
                    {
                        Username = Username,
                        Email = Email,
                        IsActive = IsActive ?? true,
                        RoleId = SelectedRole!.Id,  // กำหนดเฉพาะ RoleId ไม่ต้องกำหนด Role object เพื่อป้องกัน unique constraint
                        Profile = new UserProfile
                        {
                            FirstName = FirstName,
                            LastName = LastName,
                            PhoneNumber = PhoneNumber,
                            DateOfBirth = DateOfBirth,
                            Gender = Gender,
                            Address = Address,
                            ProfilePicture = ProfilePicture
                        }
                    };

                    // เพิ่มผู้ใช้ใหม่พร้อมรหัสผ่าน
                    var result = await _userService.RegisterAsync(newUser, NewPassword);
                    UserId = result.Id;

                    MessageBox.Show("เพิ่มผู้ใช้ใหม่เรียบร้อยแล้ว", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else // สำหรับการแก้ไขข้อมูลผู้ใช้ที่มีอยู่แล้ว
                {
                    if (User == null) return;

                    // อัพเดทข้อมูลผู้ใช้
                    User.Email = Email;
                    User.IsActive = IsActive ?? false;
                    
                    // อัพเดทบทบาทถ้าสามารถเปลี่ยนได้
                    if (CanChangeRole && SelectedRole != null && User.RoleId != SelectedRole.Id)
                    {
                        User.RoleId = SelectedRole.Id;
                    }

                    // อัพเดทรหัสผ่านถ้ามีการเปลี่ยนแปลง
                    if (IsChangePassword && !string.IsNullOrEmpty(NewPassword))
                    {
                        await _userService.ChangePasswordAsync(User.Id, NewPassword);
                    }

                    // อัพเดทข้อมูลโปรไฟล์
                    User.Profile.FirstName = FirstName;
                    User.Profile.LastName = LastName;
                    User.Profile.PhoneNumber = PhoneNumber;
                    User.Profile.DateOfBirth = DateOfBirth;
                    User.Profile.Gender = Gender;
                    User.Profile.Address = Address;
                    User.Profile.ProfilePicture = ProfilePicture;

                    // บันทึกข้อมูลผู้ใช้
                    var rowAffectedUpdateUser = await _userService.UpdateAsync(User);

                    MessageBox.Show("บันทึกข้อมูลเรียบร้อย", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);
                }

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
            bool hasChanges = FirstName != _originalFirstName ||
                LastName != _originalLastName ||
                Email != _originalEmail ||
                PhoneNumber != _originalPhoneNumber ||
                DateOfBirth != _originalDateOfBirth ||
                Gender != _originalGender ||
                Address != _originalAddress ||
                IsActive != _originalIsActive ||
                ProfilePicture != _originalProfilePicture ||
                IsChangePassword ||
                (CanChangeRole && SelectedRole != null && User?.RoleId != SelectedRole.Id);
            
            // ตรวจสอบความถูกต้องของฟอร์ม
            ValidateForm();
            
            // ต้องมีการเปลี่ยนแปลงและฟอร์มถูกต้องจึงจะกดปุ่มบันทึกได้
            IsDataChanged = hasChanges && PasswordErrorMessage == null && IsFormValid;
            
            // อัพเดทค่า CanSave เพื่อควบคุมปุ่มบันทึก
            CanSave = IsDataChanged && IsFormValid && PasswordErrorMessage == null;
        }
        
        /// <summary>
        /// ตรวจสอบความถูกต้องของฟอร์ม เพื่อให้แน่ใจว่ากรอกข้อมูลที่จำเป็นครบถ้วน
        /// </summary>
        private void ValidateForm()
        {
            bool oldFormValid = IsFormValid;
            if (IsNewUser)
            {
                // สำหรับการเพิ่มผู้ใช้ใหม่ ต้องมีข้อมูลพื้นฐานที่จำเป็น
                IsFormValid = !string.IsNullOrWhiteSpace(Username) &&
                             SelectedRole != null &&
                             !string.IsNullOrWhiteSpace(NewPassword) &&
                             (NewPassword == ConfirmPassword);
            }
            else
            {
                // สำหรับการแก้ไขผู้ใช้ที่มีอยู่แล้ว
                if (IsChangePassword)
                {
                    // ถ้ามีการเปลี่ยนรหัสผ่าน ต้องป้อนรหัสผ่านให้ถูกต้อง
                    IsFormValid = !string.IsNullOrWhiteSpace(NewPassword) &&
                                  (NewPassword == ConfirmPassword);
                }
                else
                {
                    // ถ้าไม่มีการเปลี่ยนรหัสผ่าน ถือว่าฟอร์มถูกต้อง
                    IsFormValid = true;
                }
            }
            
            // ถ้าสถานะของฟอร์มเปลี่ยน ให้อัพเดทสถานะของปุ่มบันทึกด้วย
            if (oldFormValid != IsFormValid)
            {
                CanSave = IsDataChanged && IsFormValid && PasswordErrorMessage == null;
            }
        }

        partial void OnDateOfBirthChanged(DateTime? value)
        {
            CheckForChanges();
        }

        partial void OnNewPasswordChanged(string? value)
        {
            ValidatePasswords();
            CheckForChanges();
        }

        partial void OnConfirmPasswordChanged(string? value)
        {
            ValidatePasswords();
            CheckForChanges();
        }

        partial void OnIsChangePasswordChanged(bool value)
        {
            if (!value)
            {
                NewPassword = null;
                ConfirmPassword = null;
                PasswordErrorMessage = null;
            }
            
            ValidatePasswords();
            CheckForChanges();
        }

        private void ValidatePasswords()
        {
            if (IsChangePassword)
            {
                if (string.IsNullOrEmpty(NewPassword))
                {
                    PasswordErrorMessage = "กรุณากรอกรหัสผ่าน";
                    return;
                }

                if (NewPassword?.Length < 6)
                {
                    PasswordErrorMessage = "รหัสผ่านต้องมีความยาวอย่างน้อย 6 ตัวอักษร";
                    return;
                }

                if (NewPassword != ConfirmPassword)
                {
                    PasswordErrorMessage = "รหัสผ่านไม่ตรงกัน";
                    return;
                }

                PasswordErrorMessage = null;
            }
        }

        partial void OnGenderChanged(string? value)
        {
            CheckForChanges();
        }

        partial void OnEmailChanged(string? value)
        {
            CheckForChanges();
        }

        partial void OnPhoneNumberChanged(string? value)
        {
            CheckForChanges();
        }

        partial void OnAddressChanged(string? value)
        {
            CheckForChanges();
        }

        partial void OnIsActiveChanged(bool? value)
        {
            CheckForChanges();
        }

        partial void OnSelectedRoleChanged(Role? value)
        {
            CheckForChanges();
        }
    }
}
