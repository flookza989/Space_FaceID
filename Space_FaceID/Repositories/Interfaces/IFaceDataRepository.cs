﻿using Space_FaceID.Models.Entities;
using Space_FaceID.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Repositories
{
    public interface IFaceDataRepository : IGenericRepository<FaceData>
    {
        Task<List<FaceData>> GetFaceDatasByUserIdAsync(int id);
    }
}
