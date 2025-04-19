using Space_FaceID.Models.Entities;
using Space_FaceID.Repositories.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Repositories.Interfaces
{
    public interface IUnitOfWorkRepository
    {
        ICameraSettingRepository CameraSettingRepository { get; }
        IFaceDetectionSettingRepository FaceDetectionSettingRepository { get; }
    }
}
