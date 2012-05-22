using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Treefrog.Converters
{
    public class StringToIntNull : IValueConverter
    {

        #region IValueConverter Members

        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int val;
            if (int.TryParse((string)value, out val))
                return val;
            return null;
        }

        #endregion
    }
}
