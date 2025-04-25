using System;
using System.Globalization;
using System.Windows.Data;

namespace Space_FaceID.Helpers.Converters
{
    /// <summary>
    /// แปลงค่า Boolean เป็นค่าอื่นๆ ตามที่กำหนดในพารามิเตอร์
    /// </summary>
    /// <remarks>
    /// การใช้งาน:
    /// 1. แปลงเป็นข้อความ: {Binding IsActive, Converter={StaticResource BooleanConverter}, ConverterParameter='เปิดใช้งาน|ปิดใช้งาน'}
    /// 2. แปลงเป็นตัวเลข: {Binding IsActive, Converter={StaticResource BooleanConverter}, ConverterParameter='1|0'}
    /// 3. แปลงเป็นอะไรก็ได้: {Binding IsActive, Converter={StaticResource BooleanConverter}, ConverterParameter='ValueWhenTrue|ValueWhenFalse'}
    /// </remarks>
    public class BooleanConverter : IValueConverter
    {
        /// <summary>
        /// แปลงค่า Boolean เป็นค่าอื่นๆ ตามที่กำหนดในพารามิเตอร์
        /// </summary>
        /// <param name="value">ค่า Boolean ที่จะแปลง</param>
        /// <param name="targetType">ชนิดที่ต้องการแปลงเป็น</param>
        /// <param name="parameter">พารามิเตอร์ในรูปแบบ "ValueWhenTrue|ValueWhenFalse"</param>
        /// <param name="culture">วัฒนธรรม</param>
        /// <returns>ค่าที่แปลงแล้ว</returns>
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool boolValue)
            {
                // ถ้า targetType เป็นชนิดที่ไม่ยอมรับ null ให้ส่งค่าพื้นฐานกลับไปแทน
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }

            if (parameter == null)
            {
                return boolValue ? 1 : 0;
            }

            string[] values = parameter.ToString()?.Split('|') ?? [];

            if (values.Length == 1)
            {
                return boolValue ? values[0] : string.Empty;
            }

            if (values.Length >= 2)
            {
                return boolValue ? values[0] : values[1];
            }

            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }


        /// <summary>
        /// แปลงค่ากลับเป็น Boolean
        /// </summary>
        /// <param name="value">ค่าที่จะแปลงกลับ</param>
        /// <param name="targetType">ชนิดที่ต้องการแปลงเป็น</param>
        /// <param name="parameter">พารามิเตอร์ในรูปแบบ "ValueWhenTrue|ValueWhenFalse"</param>
        /// <param name="culture">วัฒนธรรม</param>
        /// <returns>ค่า Boolean</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null)
            {
                return false; // หรือค่า default ของ bool
            }

            string stringValue = value?.ToString() ?? string.Empty;
            string[] values = parameter.ToString()?.Split('|') ?? [];

            if (values.Length >= 2)
            {
                if (stringValue == values[0])
                {
                    return true;
                }
                else if (stringValue == values[1])
                {
                    return false;
                }
            }

            return false; // fallback ปลอดภัย
        }

    }
}
