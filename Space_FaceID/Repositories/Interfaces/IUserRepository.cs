using Space_FaceID.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<List<User>> GetAllUserWithFullAsync();
        Task<User?> GetUserWithFullByUserIdAsync(int userId);
        Task<bool> UpdatePasswordAsync(int userId, string newPassword);
        Task<User> RegisterUserAsync(User user, string password);
        Task<User?> GetUserByUsernameAsync(string username);
    }
}
