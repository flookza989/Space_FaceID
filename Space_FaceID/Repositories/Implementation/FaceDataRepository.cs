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
    public class FaceDataRepository : GenericRepository<FaceData>, IFaceDataRepository
    {
        private readonly IDbContextFactory<FaceIDDbContext> _contextFactory;
        public FaceDataRepository(IDbContextFactory<FaceIDDbContext> contextFactory) : base(contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<FaceData>> GetFaceDatasByUserIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var faceDatas = await context.FaceDatas
                .Include(x => x.User)
                .Where(x => x.UserId == id)
                .ToListAsync();

            return faceDatas;
        }
    }
}
