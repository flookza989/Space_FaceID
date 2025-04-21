using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ViewFaceCore.Model;

namespace Space_FaceID.Services.Interfaces
{
    public interface IImageService
    {
        FaceImage LoadImage(string path);
        FaceImage LoadImage(byte[] imageData);
        byte[] ConvertBitmapSourceToBytes(BitmapSource bitmapSource);
        BitmapSource DrawFacesOnImage(BitmapSource originalImage, FaceInfo[]? faces);
        BitmapImage? ByteArrayToBitmapImage(byte[]? byteArray);
    }
}
