using Space_FaceID.Models.Entities;
using Space_FaceID.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Repositories.Implementation
{
    public class UnitOfWorkRepository(ICameraSettingRepository cameraSettingRepository,
        IFaceDetectionSettingRepository faceDetectionSettingRepository,
        IFaceRecognizeSettingRepository faceRecognizeSettingRepository,
        IFaceDataRepository faceDataRepository,
        IUserProfileRepository userProfileRepository,
        IUserRepository userRepository,
        IAuthenticationLogRepository authenticationLogRepository,
        IFaceAuthenticationSettingRepository faceAuthenticationSettingRepository,
        ISystemAuditLogRepository systemAuditLogRepository,
        IRoleRepository roleRepository) : IUnitOfWorkRepository
    {
        private readonly ICameraSettingRepository _cameraSettingRepository = cameraSettingRepository ?? throw new ArgumentNullException(nameof(cameraSettingRepository));
        public ICameraSettingRepository CameraSettingRepository => _cameraSettingRepository;

        private readonly IFaceDetectionSettingRepository _faceDetectionSettingRepository = faceDetectionSettingRepository ?? throw new ArgumentNullException(nameof(faceDetectionSettingRepository));
        public IFaceDetectionSettingRepository FaceDetectionSettingRepository => _faceDetectionSettingRepository;

        private readonly IFaceRecognizeSettingRepository _faceRecognizeSettingRepository = faceRecognizeSettingRepository ?? throw new ArgumentNullException(nameof(faceRecognizeSettingRepository));
        public IFaceRecognizeSettingRepository FaceRecognizeSettingRepository => _faceRecognizeSettingRepository;

        private readonly IFaceDataRepository _faceDataRepository = faceDataRepository ?? throw new ArgumentNullException(nameof(faceDataRepository));
        public IFaceDataRepository FaceDataRepository => _faceDataRepository;

        private readonly IUserProfileRepository _userProfileRepository = userProfileRepository ?? throw new ArgumentNullException(nameof(userProfileRepository));
        public IUserProfileRepository UserProfileRepository => _userProfileRepository;

        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        public IUserRepository UserRepository => _userRepository;

        private readonly IAuthenticationLogRepository _authenticationLogRepository = authenticationLogRepository ?? throw new ArgumentNullException(nameof(authenticationLogRepository));
        public IAuthenticationLogRepository AuthenticationLogRepository => _authenticationLogRepository;

        private readonly IFaceAuthenticationSettingRepository _faceAuthenticationSettingRepository = faceAuthenticationSettingRepository ?? throw new ArgumentNullException(nameof(faceAuthenticationSettingRepository));
        public IFaceAuthenticationSettingRepository FaceAuthenticationSettingRepository => _faceAuthenticationSettingRepository;

        private readonly ISystemAuditLogRepository _systemAuditLogRepository = systemAuditLogRepository ?? throw new ArgumentNullException(nameof(systemAuditLogRepository));
        public ISystemAuditLogRepository SystemAuditLogRepository => _systemAuditLogRepository;

        private readonly IRoleRepository _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        public IRoleRepository RoleRepository => _roleRepository;
    }
}
