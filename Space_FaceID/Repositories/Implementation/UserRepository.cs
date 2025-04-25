using Microsoft.EntityFrameworkCore;
using Space_FaceID.Data.Context;
using Space_FaceID.Models.Entities;
using Space_FaceID.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Repositories.Implementation
{
    public class UserRepository(IDbContextFactory<FaceIDDbContext> contextFactory) : GenericRepository<User>(contextFactory), IUserRepository
    {
        private readonly IDbContextFactory<FaceIDDbContext> _contextFactory = contextFactory;

        public async Task<List<User>> GetAllUserWithFullAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Users
                .Include(u => u.Profile)
                .Include(u => u.Role)
                .Include(u => u.FaceDatas)
                .ToListAsync();
        }

        public async Task<User?> GetUserWithFullByUserIdAsync(int userId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Users
                .Include(u => u.Profile)
                .Include(u => u.Role)
                .Include(u => u.FaceDatas)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
        {
            using var context = _contextFactory.CreateDbContext();
            var user = await context.Users.FindAsync(userId);

            if (user == null)
                return false;

            // Update the password hash using the system's encryption method
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // Save the changes
            context.Users.Update(user);
            var result = await context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<User> RegisterUserAsync(User user, string password)
        {
            using var context = _contextFactory.CreateDbContext();

            // Hash the password before storing
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // Set creation timestamp
            if (user.Profile != null)
            {
                context.UserProfiles.Add(user.Profile);
            }

            // Add the new user
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Return the user with the Id assigned by the database
            return user;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Users
                .Include(u => u.Profile)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
