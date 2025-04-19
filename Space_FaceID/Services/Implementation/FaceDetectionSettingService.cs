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
    public class FaceDetectionSettingService : GenericService<FaceDetectionSetting>, IFaceDetectionSettingService
    {
        private readonly IUnitOfWorkRepository _unitOfWorkRepository;

        public FaceDetectionSettingService(IUnitOfWorkRepository unitOfWorkRepository)
            : base(unitOfWorkRepository.FaceDetectionSettingRepository)
        {
            _unitOfWorkRepository = unitOfWorkRepository;
        }

        public async Task<FaceDetectionSetting?> GetActiveFaceDetectionSettingAsync()
        {
            return await _unitOfWorkRepository.FaceDetectionSettingRepository.GetActiveFaceDetectionSettingAsync();
        }
    }
}
