using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Helpers.Extensions
{
    public static class ResolutionExtensions
    {
        // แปลงจากสตริงเป็นขนาดเฟรม
        public static (int Width, int Height) ToFrameDimensions(this string resolution)
        {
            string[] dimensions = resolution.Split('x');

            if (dimensions.Length == 2 &&
                int.TryParse(dimensions[0], out int width) &&
                int.TryParse(dimensions[1], out int height))
            {
                return (width, height);
            }

            return (0, 0);
        }

        // แปลงจากขนาดเฟรมเป็นสตริง
        public static string ToResolutionString(this (int Width, int Height) dimensions)
        {
            return $"{dimensions.Width}x{dimensions.Height}";
        }
    }
}
