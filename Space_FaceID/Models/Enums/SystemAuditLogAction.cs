using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Models.Enums
{
    public enum SystemAuditLogAction
    {
        // User Management
        UserCreated = 1,
        UserUpdated = 2,
        UserDeleted = 3,
        UserActivated = 4,
        UserDeactivated = 5,
        UserLockedOut = 6,
        UserUnlocked = 7,
        PasswordChanged = 8,
        PasswordReset = 9,

        // Face Recognition
        FaceRegistered = 10,
        FaceUpdated = 11,
        FaceDeleted = 12,
        FaceVerificationSuccess = 13,
        FaceVerificationFailed = 14,
        LivenessCheckFailed = 15,

        // Authentication
        LoginSuccess = 20,
        LoginFailed = 21,
        Logout = 22,
        TwoFactorEnabled = 23,
        TwoFactorDisabled = 24,

        // Role & Permission
        RoleCreated = 30,
        RoleUpdated = 31,
        RoleDeleted = 32,
        RoleAssignedToUser = 33,
        RoleRemovedFromUser = 34,
        PermissionCreated = 35,
        PermissionUpdated = 36,
        PermissionDeleted = 37,
        PermissionAssignedToRole = 38,
        PermissionRemovedFromRole = 39,

        // System Settings
        SystemSettingsUpdated = 40,
        FaceRecognitionSettingsUpdated = 41,
        SecuritySettingsUpdated = 42,

        // System Operation
        SystemStartup = 50,
        SystemShutdown = 51,
        DatabaseBackup = 52,
        DatabaseRestore = 53,
        DatabaseMigration = 54,

        // Device Management
        DeviceRegistered = 60,
        DeviceUpdated = 61,
        DeviceRemoved = 62,
        DeviceConnected = 63,
        DeviceDisconnected = 64,

        // Data Operation
        DataExported = 70,
        DataImported = 71,
        ReportGenerated = 72,

        // API Access
        ApiKeyGenerated = 80,
        ApiKeyRevoked = 81,
        ApiAccessGranted = 82,
        ApiAccessDenied = 83,

        // Other
        Other = 999
    }
}
