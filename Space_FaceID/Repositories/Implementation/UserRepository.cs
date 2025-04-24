using Microsoft.EntityFrameworkCore;
using Space_FaceID.Data.Context;
using Space_FaceID.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Repositories.Implementation
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly IDbContextFactory<FaceIDDbContext> _contextFactory;

        public UserRepository(IDbContextFactory<FaceIDDbContext> contextFactory) : base(contextFactory)
        {
            _contextFactory = contextFactory;
        }

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
    }
}
