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
    public class UserService : GenericService<User>, IUserService
    {
        private readonly IUnitOfWorkRepository _unitOfWorkRepository;
        public UserService(IUnitOfWorkRepository unitOfWorkRepository) : base(unitOfWorkRepository.UserRepository)
        {
            _unitOfWorkRepository = unitOfWorkRepository;
        }

        public async Task<List<User>> GetAllUserWithFullAsync()
        {
            return await _unitOfWorkRepository.UserRepository.GetAllUserWithFullAsync();
        }

        public async Task<User?> GetUserWithFullByUserIdAsync(int userId)
        {
            return await _unitOfWorkRepository.UserRepository.GetUserWithFullByUserIdAsync(userId);
        }
    }
}
