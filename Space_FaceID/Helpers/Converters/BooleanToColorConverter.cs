using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Space_FaceID.Helpers.Converters
{
    public class BooleanToColorConverter : IValueConverter
    {
        public string TrueColor { get; set; } = "#4CAF50";
        public string FalseColor { get; set; } = "#F44336";

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                string colorStr = boolValue ? TrueColor : FalseColor;
                return new BrushConverter().ConvertFrom(colorStr) as SolidColorBrush;
            }
            return Brushes.Gray;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
