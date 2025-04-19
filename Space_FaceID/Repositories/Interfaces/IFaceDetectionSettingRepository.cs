using Space_FaceID.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Repositories.Interfaces
{
    public interface IFaceDetectionSettingRepository : IGenericRepository<FaceDetectionSetting>
    {
        Task<FaceDetectionSetting?> GetActiveFaceDetectionSettingAsync();
    }
}
