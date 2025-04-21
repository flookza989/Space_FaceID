using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Space_FaceID.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using ViewFaceCore;
using ViewFaceCore.Model;

namespace Space_FaceID.Services.Implementation
{
    public class ImageService : IImageService
    {
        public FaceImage LoadImage(string path)
        {
            using var image = Image.Load<Rgba32>(path);
            return image.ToFaceImage();
        }

        public FaceImage LoadImage(byte[] imageData)
        {
            using var image = Image.Load<Rgba32>(imageData);
            return image.ToFaceImage();
        }

        public byte[] ConvertBitmapSourceToBytes(BitmapSource bitmapSource)
        {
            using var memoryStream = new MemoryStream();
            var encoder = new JpegBitmapEncoder(); // หรือ PngBitmapEncoder ตามที่ต้องการ
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(memoryStream);
            return memoryStream.ToArray();
        }

        public BitmapSource DrawFacesOnImage(BitmapSource originalImage, FaceInfo[]? faces)
        {
            // สร้าง DrawingVisual เพื่อวาดบนภาพ
            DrawingVisual drawingVisual = new();

            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                // วาดภาพต้นฉบับก่อน
                drawingContext.DrawImage(originalImage, new Rect(0, 0, originalImage.Width, originalImage.Height));

                // ตรวจสอบว่า faces ไม่ใช่ null ก่อนทำการวนลูป
                if (faces != null)
                {
                    // วาดกรอบสี่เหลี่ยมรอบใบหน้าแต่ละใบหน้า
                    foreach (var face in faces)
                    {
                        // สร้างกรอบสี่เหลี่ยม
                        Rect faceRect = new(face.Location.X, face.Location.Y,
                                            face.Location.Width, face.Location.Height);

                        // กำหนดสีและความหนาของเส้น
                        Pen pen = new(new SolidColorBrush(Colors.LimeGreen), 2);

                        // วาดกรอบสี่เหลี่ยม
                        drawingContext.DrawRectangle(null, pen, faceRect);

                        // อาจเพิ่มข้อความแสดงความเชื่อมั่น (confidence)
                        FormattedText text = new(
                            $"Confidence: {face.Score:F2}",
                            CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight,
                            new Typeface("Arial"),
                            12,
                            Brushes.LimeGreen,
                            VisualTreeHelper.GetDpi(drawingVisual).PixelsPerDip);

                        drawingContext.DrawText(text, new System.Windows.Point(face.Location.X, face.Location.Y - 15));
                    }
                }
            }

            // แปลง DrawingVisual เป็น BitmapSource
            RenderTargetBitmap renderBitmap = new(
                (int)originalImage.Width, (int)originalImage.Height,
                96, 96, PixelFormats.Pbgra32);

            renderBitmap.Render(drawingVisual);
            renderBitmap.Freeze(); // สำคัญ: ทำให้สามารถส่งระหว่างเธรดได้

            return renderBitmap;
        }

        public BitmapImage? ByteArrayToBitmapImage(byte[]? byteArray)
        {
            if (byteArray == null || byteArray.Length == 0) return null;

            try
            {
                using MemoryStream stream = new(byteArray);
                BitmapImage image = new();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze(); // ทำให้ thread-safe
                return image;
            }
            catch
            {
                return null;
            }
        }
    }
}
