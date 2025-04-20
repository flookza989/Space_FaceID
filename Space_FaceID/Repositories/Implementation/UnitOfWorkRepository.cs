using Space_FaceID.Models.Entities;
using Space_FaceID.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Repositories.Implementation
{
    public class UnitOfWorkRepository : IUnitOfWorkRepository
    {
        private readonly ICameraSettingRepository _cameraSettingRepository;
        public ICameraSettingRepository CameraSettingRepository => _cameraSettingRepository;

        private readonly IFaceDetectionSettingRepository _faceDetectionSettingRepository;
        public IFaceDetectionSettingRepository FaceDetectionSettingRepository => _faceDetectionSettingRepository;

        private readonly IFaceRecognizeSettingRepository _faceRecognizeSettingRepository;
        public IFaceRecognizeSettingRepository FaceRecognizeSettingRepository => _faceRecognizeSettingRepository;

        private readonly IFaceDataRepository _faceDataRepository;
        public IFaceDataRepository FaceDataRepository => _faceDataRepository;

        private readonly IUserProfileRepository _userProfileRepository;
        public IUserProfileRepository UserProfileRepository => _userProfileRepository;

        private readonly IUserRepository _userRepository;
        public IUserRepository UserRepository => _userRepository;

        public UnitOfWorkRepository(ICameraSettingRepository cameraSettingRepository,
            IFaceDetectionSettingRepository faceDetectionSettingRepository,
            IFaceRecognizeSettingRepository faceRecognizeSettingRepository,
            IFaceDataRepository faceDataRepository,
            IUserProfileRepository userProfileRepository,
            IUserRepository userRepository)
        {
            _cameraSettingRepository = cameraSettingRepository ?? throw new ArgumentNullException(nameof(cameraSettingRepository));
            _faceDetectionSettingRepository = faceDetectionSettingRepository ?? throw new ArgumentNullException(nameof(faceDetectionSettingRepository));
            _faceRecognizeSettingRepository = faceRecognizeSettingRepository ?? throw new ArgumentNullException(nameof(faceRecognizeSettingRepository));
            _faceDataRepository = faceDataRepository ?? throw new ArgumentNullException(nameof(faceDataRepository));
            _userProfileRepository = userProfileRepository ?? throw new ArgumentNullException(nameof(userProfileRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }



    }
}
