using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Space_FaceID.Helpers.Converters
{
    public class BooleanToTextActiveConverter : IValueConverter
    {
        public string TrueValue { get; set; } = "ใช้งาน";
        public string FalseValue { get; set; } = "ไม่ใช้งาน";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueValue : FalseValue;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
