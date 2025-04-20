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
    public class UserProfileRepository : GenericRepository<UserProfile>, IUserProfileRepository
    {
        private readonly IDbContextFactory<FaceIDDbContext> _contextFactory;

        public UserProfileRepository(IDbContextFactory<FaceIDDbContext> contextFactory) : base(contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<UserProfile?> GetUserProfileByUserIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var userProfile = await context.UserProfiles
                .Include(up => up.User)
                .FirstOrDefaultAsync(up => up.UserId == id);

            return userProfile;
        }
    }
}
