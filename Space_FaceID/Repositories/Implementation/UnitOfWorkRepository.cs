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
        public UnitOfWorkRepository(ICameraSettingRepository cameraSettingRepository, IFaceDetectionSettingRepository faceDetectionSettingRepository)
        {
            _cameraSettingRepository = cameraSettingRepository;
            _faceDetectionSettingRepository = faceDetectionSettingRepository;
        }



    }
}
