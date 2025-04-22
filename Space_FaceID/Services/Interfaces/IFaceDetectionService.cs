using Space_FaceID.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Services.Interfaces
{
    public interface IFaceDetectionService
    {
        Task<FaceDetectionResult> DetectFromFileAsync(string imagePath);
        Task<FaceDetectionResult> DetectFromBytesAsync(byte[] imageData);
        Task LoadConfigurationFaceDetectorAsync();
    }
}
