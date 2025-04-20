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
    public class FaceRecognizeSettingService : GenericService<FaceRecognizeSetting>, IFaceRecognizeSettingService
    {
        private readonly IUnitOfWorkRepository _unitOfWorkRepository;
        public FaceRecognizeSettingService(IUnitOfWorkRepository unitOfWorkRepository)
            : base(unitOfWorkRepository.FaceRecognizeSettingRepository)
        {
            _unitOfWorkRepository = unitOfWorkRepository;
        }
        public async Task<FaceRecognizeSetting?> GetActiveFaceRecognizeSettingAsync()
        {
            return await _unitOfWorkRepository.FaceRecognizeSettingRepository.GetActiveFaceRecognizeSettingAsync();
        }
    }
}
