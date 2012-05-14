using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Treefrog.V2.ViewModel;

namespace Treefrog.V2.View.Panels
{
    /// <summary>
    /// Interaction logic for ObjectPanel.xaml
    /// </summary>
    public partial class ObjectPanel : UserControl
    {
        public ObjectPanel ()
        {
            InitializeComponent();
        }
    }

    [ValueConversion(typeof(PreviewSize), typeof(string))]
    public class PreviewSizeToIconFilenameConverter : IValueConverter
    {
        private static string IconPath = "/Treefrog.V2;component/Images/16/";

        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((PreviewSize)value) {
                case PreviewSize.Large:
                    return IconPath + "icons-large.png";
                case PreviewSize.Medium:
                    return IconPath + "icons-medium.png";
                case PreviewSize.Small:
                    return IconPath + "icons-small.png";
                default:
                    return null;
            }
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(PreviewSize), typeof(string))]
    public class PreviewSizeToTooltipConverter : IValueConverter
    {
        private static string IconPath = "/Treefrog.V2;component/Images/16/";

        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((PreviewSize)value) {
                case PreviewSize.Large:
                    return "Show large icons";
                case PreviewSize.Medium:
                    return "Show medium icons";
                case PreviewSize.Small:
                    return "Show small icons";
                default:
                    return null;
            }
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(PreviewSize), typeof(int))]
    public class PreviewSizeToDimensionConverter : IValueConverter
    {
        private static string IconPath = "/Treefrog.V2;component/Images/16/";

        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((PreviewSize)value) {
                case PreviewSize.Large:
                    return 96;
                case PreviewSize.Medium:
                    return 64;
                case PreviewSize.Small:
                    return 32;
                default:
                    return null;
            }
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(ObjectSnappingSource), typeof(string))]
    public class SnappingSourceToIconFilenameConverter : IValueConverter
    {
        private static string IconPath = "/Treefrog.V2;component/Images/16/";

        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((ObjectSnappingSource)value) {
                case ObjectSnappingSource.ImageBounds:
                    return IconPath + "snap-borders.png";
                case ObjectSnappingSource.MaskBounds:
                    return IconPath + "snap-bounds.png";
                case ObjectSnappingSource.Origin:
                    return IconPath + "snap-origin.png";
                default:
                    return null;
            }
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(ObjectSnappingSource), typeof(string))]
    public class SnappingSourceToTooltipConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((ObjectSnappingSource)value) {
                case ObjectSnappingSource.ImageBounds:
                    return "Snap to Image Bounds";
                case ObjectSnappingSource.MaskBounds:
                    return "Snap to Mask Bounds";
                case ObjectSnappingSource.Origin:
                    return "Snap to Origin";
                default:
                    return null;
            }
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(ObjectSnappingTarget), typeof(string))]
    public class SnappingTargetToIconFilenameConverter : IValueConverter
    {
        private static string IconPath = "/Treefrog.V2;component/Images/16/";

        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((ObjectSnappingTarget)value)
	        {
		        case ObjectSnappingTarget.None:
                    return IconPath + "snap-none.png";
                case ObjectSnappingTarget.TopLeft:
                    return IconPath + "snap-topleft.png";
                case ObjectSnappingTarget.TopRight:
                    return IconPath + "snap-topright.png";
                case ObjectSnappingTarget.BottomLeft:
                    return IconPath + "snap-bottomleft.png";
                case ObjectSnappingTarget.BottomRight:
                    return IconPath + "snap-bottomright.png";
                case ObjectSnappingTarget.Top:
                    return IconPath + "snap-top.png";
                case ObjectSnappingTarget.Bottom:
                    return IconPath + "snap-bottom.png";
                case ObjectSnappingTarget.Left:
                    return IconPath + "snap-left.png";
                case ObjectSnappingTarget.Right:
                    return IconPath + "snap-right.png";
                case ObjectSnappingTarget.CenterHorizontal:
                    return IconPath + "snap-horizontal.png";
                case ObjectSnappingTarget.CenterVertical:
                    return IconPath + "snap-vertical.png";
                case ObjectSnappingTarget.Center:
                    return IconPath + "snap-center.png";
                default:
                    return null;
	        }
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(ObjectSnappingTarget), typeof(string))]
    public class SnappingTargetToTooltipConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((ObjectSnappingTarget)value) {
                case ObjectSnappingTarget.None:
                    return "Do not snap";
                case ObjectSnappingTarget.TopLeft:
                    return "Snap to top-left corner";
                case ObjectSnappingTarget.TopRight:
                    return "Snap to top-right corner";
                case ObjectSnappingTarget.BottomLeft:
                    return "Snap to bottom-left corner";
                case ObjectSnappingTarget.BottomRight:
                    return "Snap to bottom-right corner";
                case ObjectSnappingTarget.Top:
                    return "Snap to top edge";
                case ObjectSnappingTarget.Bottom:
                    return "Snap to bottom edge";
                case ObjectSnappingTarget.Left:
                    return "Snap to left edge";
                case ObjectSnappingTarget.Right:
                    return "Snap to right edge";
                case ObjectSnappingTarget.CenterHorizontal:
                    return "Snap between top and bottom edges";
                case ObjectSnappingTarget.CenterVertical:
                    return "Snap between left and right edges";
                case ObjectSnappingTarget.Center:
                    return "Snap to center";
                default:
                    return null;
            }
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
