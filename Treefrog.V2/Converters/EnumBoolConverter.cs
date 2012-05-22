using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Treefrog.Converters
{
    [ValueConversion(typeof(Enum), typeof(bool))]
    class EnumBoolConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string paramString = parameter as string;
            if (paramString == null)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object paramValue = Enum.Parse(value.GetType(), paramString);
            return paramValue.Equals(value);
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string paramString = parameter as string;
            if (paramString == null)
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, paramString);
        }
    }
}
