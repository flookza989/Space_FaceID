using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Space_FaceID.Services.Interfaces
{
    public interface ICameraService : IDisposable
    {
        event EventHandler<BitmapSource>? NewFrameAvailable;
        Task<bool> StartCameraAsync(int cameraIndex = 0);
        void StopCamera();
        BitmapSource? CaptureFrame();
        BitmapSource? GetCurrentFrame();
        bool IsRunning { get; }
        Task<List<int>> FindConnectedCamerasAsync();
    }
}
