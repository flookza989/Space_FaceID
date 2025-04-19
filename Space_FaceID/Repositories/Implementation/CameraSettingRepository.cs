﻿using Microsoft.EntityFrameworkCore;
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
    public class CameraSettingRepository : GenericRepository<CameraSetting>, ICameraSettingRepository
    {
        private readonly IDbContextFactory<FaceIDDbContext> _contextFactory;
        public CameraSettingRepository(IDbContextFactory<FaceIDDbContext> contextFactory) : base(contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<CameraSetting?> GetActiveCameraSettingAsync()
        {
            var context = _contextFactory.CreateDbContext();
            var activeSetting = await context.CameraSettings
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();
            return activeSetting;
        }
    }
}
