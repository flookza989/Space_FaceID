using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Space_FaceID.Data.Context;
using Space_FaceID.Models.Entities;
using Space_FaceID.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            await SeedPermissions(context);
            await SeedRolePermissions(context);
            await SeedDefaultSettings(context);
            await SeedAdminUser(context);
            await SeedFaceDetectionSetting(context);
            await SeedCameraSetting(context);

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
                    Name = RoleName.Admin,
                    Description = "ผู้ดูแลระบบที่มีสิทธิ์ทั้งหมด",
                    IsDefault = false,
                    IsSystem = true,
                    CreatedAt = DateTime.Now
                },
                new Role
                {
                    Name = RoleName.User,
                    Description = "ผู้ใช้งานทั่วไป",
                    IsDefault = true,
                    IsSystem = true,
                    CreatedAt = DateTime.Now
                },
                new Role
                {
                    Name = RoleName.Supervisor,
                    Description = "ผู้ดูแลที่มีสิทธิ์บางส่วน",
                    IsDefault = false,
                    IsSystem = true,
                    CreatedAt = DateTime.Now
                }
            };

            await context.Roles.AddRangeAsync(roles);
        }

        private static async Task SeedPermissions(FaceIDDbContext context)
        {
            if (await context.Permissions.AnyAsync())
                return;

            var permissions = new Permission[]
            {
                // User Management
                new Permission { Name = PermissionName.UsersView, Description = "ดูข้อมูลผู้ใช้", Category = PermissionCategory.UserManagement, IsSystem = true },
                new Permission { Name = PermissionName.UsersCreate, Description = "สร้างผู้ใช้ใหม่", Category = PermissionCategory.UserManagement, IsSystem = true },
                new Permission { Name = PermissionName.UsersEdit, Description = "แก้ไขข้อมูลผู้ใช้", Category = PermissionCategory.UserManagement, IsSystem = true },
                new Permission { Name = PermissionName.UsersDelete, Description = "ลบผู้ใช้", Category = PermissionCategory.UserManagement, IsSystem = true },
                
                // Face Recognition
                new Permission { Name = PermissionName.FaceIdRegister, Description = "ลงทะเบียนใบหน้า", Category = PermissionCategory.FaceRecognition, IsSystem = true },
                new Permission { Name = PermissionName.FaceIdVerify, Description = "ยืนยันตัวตนด้วยใบหน้า", Category = PermissionCategory.FaceRecognition, IsSystem = true },
                
                // Settings
                new Permission { Name = PermissionName.FaceIdSettings, Description = "ปรับแต่งการตั้งค่าระบบยืนยันตัวตน", Category = PermissionCategory.Settings, IsSystem = true },
                new Permission { Name = PermissionName.SystemSettings, Description = "ปรับแต่งการตั้งค่าระบบ", Category = PermissionCategory.Settings, IsSystem = true },
                
                // Logs & Reporting
                new Permission { Name = PermissionName.ViewLogs, Description = "ดูข้อมูลบันทึกระบบ", Category = PermissionCategory.LogsAndReporting, IsSystem = true },
                new Permission { Name = PermissionName.ExportReports, Description = "ส่งออกรายงาน", Category = PermissionCategory.LogsAndReporting, IsSystem = true }
            };

            await context.Permissions.AddRangeAsync(permissions);
        }

        private static async Task SeedRolePermissions(FaceIDDbContext context)
        {
            // ต้องมั่นใจว่ามีการบันทึก Roles และ Permissions ก่อน
            await context.SaveChangesAsync();

            if (await context.RolePermissions.AnyAsync())
                return;

            // ค้นหา RoleId ของ Admin
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleName.Admin);
            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleName.User);
            var supervisorRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleName.Supervisor);

            if (adminRole == null || userRole == null || supervisorRole == null)
                return;

            // ค้นหา PermissionId ทั้งหมด
            var permissions = await context.Permissions.ToListAsync();
            if (!permissions.Any())
                return;

            // สร้าง RolePermissions สำหรับ Admin (มีสิทธิ์ทั้งหมด)
            var adminPermissions = permissions.Select(p => new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = p.Id,
                CreatedAt = DateTime.Now,
                CreatedBy = "System"
            }).ToList();

            await context.RolePermissions.AddRangeAsync(adminPermissions);

            // สร้าง RolePermissions สำหรับ User (มีสิทธิ์จำกัด)
            var userPermissionNames = new[] { PermissionName.FaceIdRegister, PermissionName.FaceIdVerify };
            var userPermissions = permissions
                .Where(p => userPermissionNames.Contains(p.Name))
                .Select(p => new RolePermission
                {
                    RoleId = userRole.Id,
                    PermissionId = p.Id,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                }).ToList();

            await context.RolePermissions.AddRangeAsync(userPermissions);

            // สร้าง RolePermissions สำหรับ Supervisor
            var supervisorPermissionNames = new[] {
                PermissionName.UsersView, PermissionName.UsersCreate, PermissionName.UsersEdit,
                PermissionName.FaceIdRegister, PermissionName.FaceIdVerify,
                PermissionName.ViewLogs, PermissionName.ExportReports
            };
            var supervisorPermissions = permissions
                .Where(p => supervisorPermissionNames.Contains(p.Name))
                .Select(p => new RolePermission
                {
                    RoleId = supervisorRole.Id,
                    PermissionId = p.Id,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                }).ToList();

            await context.RolePermissions.AddRangeAsync(supervisorPermissions);
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

            // สร้างผู้ใช้ Admin
            var adminUser = new User
            {
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
                Gender = "Other",
            };

            await context.UserProfiles.AddAsync(adminProfile);

            // หา Admin Role
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleName.Admin);
            if (adminRole == null)
                return;

            // สร้าง UserRole สำหรับ Admin
            var userRole = new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                CreatedAt = DateTime.Now,
                CreatedBy = "System"

            };

            await context.UserRoles.AddAsync(userRole);
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
    }
}
