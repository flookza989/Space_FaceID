using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Space_FaceID.Helpers.Converters
{
    public class NotNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ไม่จำเป็นต้องทำ convert back ในกรณีนี้
            throw new NotImplementedException();
        }
    }
}
