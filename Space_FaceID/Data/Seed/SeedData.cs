using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Space_FaceID.Data.Context;
using Space_FaceID.Models.Entities;
using Space_FaceID.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using ViewFaceCore.Model;

namespace Space_FaceID.Data.Seed
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = new FaceIDDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<FaceIDDbContext>>());

            // สร้างข้อมูลเริ่มต้น
            await SeedRoles(context);
            await SeedDefaultSettings(context);
            await SeedAdminUser(context);
            await SeedFaceDetectionSetting(context);
            await SeedCameraSetting(context);
            await SeedFaceRecognizeSetting(context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedRoles(FaceIDDbContext context)
        {
            if (await context.Roles.AnyAsync())
                return;

            var roles = new Role[]
            {
                new Role
                {
                    Name = RoleName.Admin.ToString(),
                    Description = "ผู้ดูแลระบบที่มีสิทธิ์ทั้งหมด",
                    IsDefault = false,
                    IsSystem = true,
                    CreatedAt = DateTime.Now
                },
                new Role
                {
                    Name = RoleName.User.ToString(),
                    Description = "ผู้ใช้งานทั่วไป",
                    IsDefault = true,
                    IsSystem = true,
                    CreatedAt = DateTime.Now
                },
                new Role
                {
                    Name = RoleName.Supervisor.ToString(),
                    Description = "ผู้ดูแลที่มีสิทธิ์บางส่วน",
                    IsDefault = false,
                    IsSystem = true,
                    CreatedAt = DateTime.Now
                }
            };

            await context.Roles.AddRangeAsync(roles);
        }

        private static async Task SeedDefaultSettings(FaceIDDbContext context)
        {
            if (await context.FaceAuthenticationSettings.AnyAsync())
                return;

            var settings = new FaceAuthenticationSetting
            {
                MatchThreshold = 0.6f,  // ค่าความเหมือนขั้นต่ำที่ยอมรับได้ (0.6 = 60%)
                RequireLivenessCheck = true,  // ต้องการการตรวจสอบว่าเป็นใบหน้าจริงไม่ใช่รูปถ่าย
                MaxAttempts = 3,  // จำนวนครั้งสูงสุดที่ล้มเหลวก่อนถูกล็อค
                IsEnabled = true,  // เปิดใช้ระบบยืนยันตัวตนด้วยใบหน้า
                LastUpdated = DateTime.Now,
                UpdatedBy = "System"
            };

            await context.FaceAuthenticationSettings.AddAsync(settings);
        }

        private static async Task SeedAdminUser(FaceIDDbContext context)
        {
            if (await context.Users.AnyAsync(u => u.Username == "admin"))
                return;

            // หา Admin Role
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleName.Admin.ToString());
            if (adminRole == null)
                return;

            // สร้างผู้ใช้ Admin
            var adminUser = new User
            {
                RoleId = adminRole.Id,  //  RoleIdAdmin
                Username = "admin",
                Email = "admin@spacefaceid.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                IsActive = true
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();  // บันทึกเพื่อให้ได้ UserId

            // สร้างโปรไฟล์สำหรับ Admin
            var adminProfile = new UserProfile
            {
                UserId = adminUser.Id,
                FirstName = "Admin",
                LastName = "User",
                PhoneNumber = "1234567890",
                Address = "123 Admin Street, Admin City, Admin Country",
                DateOfBirth = DateTime.Now,
                Gender = "อื่นๆ",
            };

            await context.UserProfiles.AddAsync(adminProfile);
        }

        private static async Task SeedFaceDetectionSetting(FaceIDDbContext context)
        {
            if (await context.FaceDetectionSettings.AnyAsync())
                return;
            var settings = new FaceDetectionSetting
            {
                FaceSize = 20,  // ขนาดของใบหน้าที่จะตรวจจับ (ในพิกเซล)
                DetectionThreshold = 0.9f,  // ค่าความไวในการตรวจจับใบหน้า
                MaxWidth = 2000,  // ความกว้างสูงสุดของภาพที่ใช้ในการตรวจจับ
                MaxHeight = 2000,  // ความสูงสูงสุดของภาพที่ใช้ในการตรวจจับ
                IsEnabled = true,
                LastUpdated = DateTime.Now,
                UpdatedBy = "System"
            };
            await context.FaceDetectionSettings.AddAsync(settings);
        }

        private static async Task SeedCameraSetting(FaceIDDbContext context)
        {
            if (await context.CameraSettings.AnyAsync())
                return;
            var settings = new CameraSetting
            {
                CameraIndex = 0,  // ดัชนีกล้องที่ใช้ (0 = กล้องหลัก)
                FrameWidth = 640,  // ความกว้างของภาพ
                FrameHeight = 480,  // ความสูงของภาพ
                FrameRate = 24,  // อัตราเฟรมต่อวินาที
                LastUpdated = DateTime.Now,
                UpdatedBy = "System"
            };
            await context.CameraSettings.AddAsync(settings);
        }

        private static async Task SeedFaceRecognizeSetting(FaceIDDbContext context)
        {
            if (await context.FaceRecognizeSettings.AnyAsync())
                return;

            var settings = new FaceRecognizeSetting
            {
                Name = FaceType.Normal.ToString(),
                LandmarkType = FaceType.Normal.ToString(),
                RecognizerType = FaceType.Normal.ToString(),
                RecognizeThreshold = 0.62f,
                IsEnabled = true,
                LastUpdated = DateTime.Now,
                UpdatedBy = "System"
            };
            await context.FaceRecognizeSettings.AddAsync(settings);

            var lightSetting = new FaceRecognizeSetting
            {
                Name = FaceType.Light.ToString(),
                LandmarkType = FaceType.Light.ToString(),
                RecognizerType = FaceType.Light.ToString(),
                RecognizeThreshold = 0.55f,
                IsEnabled = false,
                LastUpdated = DateTime.Now,
                UpdatedBy = "System"
            };
            await context.FaceRecognizeSettings.AddAsync(lightSetting);

            var maskSetting = new FaceRecognizeSetting
            {
                Name = FaceType.Mask.ToString(),
                LandmarkType = FaceType.Mask.ToString(),
                RecognizerType = FaceType.Mask.ToString(),
                RecognizeThreshold = 0.4f,
                IsEnabled = false,
                LastUpdated = DateTime.Now,
                UpdatedBy = "System"
            };
            await context.FaceRecognizeSettings.AddAsync(maskSetting);
        }
    }
}
