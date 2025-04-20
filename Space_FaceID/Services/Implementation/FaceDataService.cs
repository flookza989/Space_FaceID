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
    public class FaceDataService : GenericService<FaceData>, IFaceDataService
    {
        private readonly IUnitOfWorkRepository _unitOfWorkRepository;
        public FaceDataService(IUnitOfWorkRepository unitOfWorkRepository)
            : base(unitOfWorkRepository.FaceDataRepository)
        {
            _unitOfWorkRepository = unitOfWorkRepository;
        }

        public async Task<List<FaceData>> GetFaceDatasByUserIdAsync(int id)
        {
            return await _unitOfWorkRepository.FaceDataRepository.GetFaceDatasByUserIdAsync(id);
        }
    }
}
