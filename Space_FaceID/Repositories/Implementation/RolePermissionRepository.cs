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
    public class RolePermissionRepository : GenericRepository<RolePermission>, IRolePermissionRepository
    {
        private readonly IDbContextFactory<FaceIDDbContext> _contextFactory;

        public RolePermissionRepository(IDbContextFactory<FaceIDDbContext> contextFactory) : base(contextFactory)
        {
            _contextFactory = contextFactory;
        }
    }
}
