using Microsoft.EntityFrameworkCore;
using Space_FaceID.Data.Context;
using Space_FaceID.Models.Entities;
using Space_FaceID.Models.Enums;
using Space_FaceID.Repositories.Interfaces;
using Space_FaceID.Services.Implementation;
using Space_FaceID.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Repositories.Implementation
{
    public class RoleRepository(IDbContextFactory<FaceIDDbContext> contextFactory) : GenericRepository<Role>(contextFactory), IRoleRepository
    {
        private readonly IDbContextFactory<FaceIDDbContext> _contextFactory = contextFactory;

        public async Task<List<Role>> GetRolesWithoutAdminAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Roles
                .Where(r => r.Name != RoleName.Admin.ToString())
                .ToListAsync();
        }
    }
}
