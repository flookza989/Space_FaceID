using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Services.Interfaces
{
    public interface IUnitOfWorkService
    {
        ICameraService CameraService { get; }
        ICameraSettingService CameraSettingService { get; }
        IFaceDetectionSettingService FaceDetectionSettingService { get; }
    }
}
