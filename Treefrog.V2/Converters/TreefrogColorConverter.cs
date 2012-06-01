using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace Treefrog.Converters
{
    [ValueConversion(typeof(Treefrog.Framework.Imaging.Color), typeof(SolidColorBrush))]
    public class TreefrogColorConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Treefrog.Framework.Imaging.Color c = (Treefrog.Framework.Imaging.Color)value;
            return new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Color c = ((SolidColorBrush)value).Color;
            return new Treefrog.Framework.Imaging.Color(c.R, c.G, c.B, c.A);
        }
    }
}
