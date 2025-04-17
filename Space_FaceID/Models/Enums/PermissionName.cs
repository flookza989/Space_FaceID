using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Models.Enums
{
    public enum PermissionName
    {
        // User Management
        UsersView = 1,
        UsersCreate = 2,
        UsersEdit = 3,
        UsersDelete = 4,

        // Face Recognition
        FaceIdRegister = 5,
        FaceIdVerify = 6,

        // Settings
        FaceIdSettings = 7,
        SystemSettings = 8,

        // Logs & Reporting
        ViewLogs = 9,
        ExportReports = 10
    }
}
