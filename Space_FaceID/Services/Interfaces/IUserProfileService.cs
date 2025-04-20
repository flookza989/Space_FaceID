using Space_FaceID.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Services.Interfaces
{
    public interface IUserProfileService : IGenericService<UserProfile>
    {
        Task<UserProfile?> GetUserProfileByUserIdAsync(int id);
    }
}
