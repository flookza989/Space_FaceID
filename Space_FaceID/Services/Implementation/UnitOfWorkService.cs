using Space_FaceID.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Services.Implementation
{
    public class UnitOfWorkService : IUnitOfWorkService
    {
        private readonly ICameraService _cameraService;
        public ICameraService CameraService => _cameraService;

        private readonly ICameraSettingService _cameraSettingService;
        public ICameraSettingService CameraSettingService => _cameraSettingService;

        private readonly IFaceDetectionSettingService _faceDetectionSettingService;
        public IFaceDetectionSettingService FaceDetectionSettingService => _faceDetectionSettingService;

        public UnitOfWorkService(
            ICameraService cameraService,
            ICameraSettingService cameraSettingService,
            IFaceDetectionSettingService faceDetectionSettingService)
        {
            _cameraService = cameraService;
            _cameraSettingService = cameraSettingService;
            _faceDetectionSettingService = faceDetectionSettingService;
        }
    }
}
