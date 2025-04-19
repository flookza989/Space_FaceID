using Space_FaceID.Models.Entities;
using Space_FaceID.Repositories.Interfaces;
using Space_FaceID.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Services.Implementation
{
    public class CameraSettingService : GenericService<CameraSetting>, ICameraSettingService
    {
        private readonly IUnitOfWorkRepository _unitOfWorkRepository;

        public CameraSettingService(IUnitOfWorkRepository unitOfWorkRepository)
            : base(unitOfWorkRepository.CameraSettingRepository)
        {
            _unitOfWorkRepository = unitOfWorkRepository;
        }

        public async Task<CameraSetting?> GetActiveCameraSettingAsync()
        {
            return await _unitOfWorkRepository.CameraSettingRepository.GetActiveCameraSettingAsync();
        }
    }
}
