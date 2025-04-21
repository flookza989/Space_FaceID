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
    public class SystemAuditLogRepository : GenericRepository<SystemAuditLog>, ISystemAuditLogRepository
    {
        private readonly IDbContextFactory<FaceIDDbContext> _contextFactory;

        public SystemAuditLogRepository(IDbContextFactory<FaceIDDbContext> contextFactory) : base(contextFactory)
        {
            _contextFactory = contextFactory;
        }
    }
}
