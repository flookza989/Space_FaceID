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
    public class UserService(IUnitOfWorkRepository unitOfWorkRepository) : GenericService<User>(unitOfWorkRepository.UserRepository), IUserService
    {
        public async Task<List<User>> GetAllUserWithFullAsync()
        {
            return await unitOfWorkRepository.UserRepository.GetAllUserWithFullAsync();
        }

        public async Task<User?> GetUserWithFullByUserIdAsync(int userId)
        {
            return await unitOfWorkRepository.UserRepository.GetUserWithFullByUserIdAsync(userId);
        }

        public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
        {
            return await unitOfWorkRepository.UserRepository.UpdatePasswordAsync(userId, newPassword);
        }

        public async Task<User> RegisterAsync(User user, string password)
        {
            return await unitOfWorkRepository.UserRepository.RegisterUserAsync(user, password);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await unitOfWorkRepository.UserRepository.GetUserByUsernameAsync(username);
        }
    }
}
