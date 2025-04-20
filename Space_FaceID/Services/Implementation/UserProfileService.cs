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
    public class UserProfileService : GenericService<UserProfile>, IUserProfileService
    {
        private readonly IUnitOfWorkRepository _unitOfWorkRepository;
        public UserProfileService(IUnitOfWorkRepository unitOfWorkRepository)
            : base(unitOfWorkRepository.UserProfileRepository)
        {
            _unitOfWorkRepository = unitOfWorkRepository;
        }

        public async Task<UserProfile?> GetUserProfileByUserIdAsync(int id)
        {
            return await _unitOfWorkRepository.UserProfileRepository.GetUserProfileByUserIdAsync(id);
        }
    }
}
