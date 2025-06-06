﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Space_FaceID.Helpers.Converters
{
    [ValueConversion(typeof(FrameworkElement), typeof(ImageSource))]
    public class VisualToImageSourceConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FrameworkElement element)
            {
                element.Measure(new Size(element.Width, element.Height));
                element.Arrange(new Rect(new Size(element.Width, element.Height)));
                var rtb = new RenderTargetBitmap((int)Math.Ceiling(element.ActualWidth),
                    (int)Math.Ceiling(element.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                rtb.Render(element);

                return rtb;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
