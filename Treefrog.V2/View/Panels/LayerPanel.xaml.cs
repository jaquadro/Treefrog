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
    /// Interaction logic for LayerPanel.xaml
    /// </summary>
    public partial class LayerPanel : UserControl
    {
        public LayerPanel ()
        {
            InitializeComponent();
        }

        private void _nameLabel_MouseDown (object sender, MouseButtonEventArgs e)
        {
            /*if (e.ClickCount != 2)
                return;

            TextBlock block = sender as TextBlock;
            Grid grid = block.Parent as Grid;

            foreach (UIElement elem in grid.Children) {
                if (elem is TextBlock)
                    (elem as TextBlock).Visibility = System.Windows.Visibility.Collapsed;
                else if (elem is TextBox) {
                    (elem as TextBox).Visibility = System.Windows.Visibility.Visible;
                    //elem.Focus();
                    //Keyboard.Focus(elem as TextBox);
                }
            }*/
        }

        private void TextBox_LostFocus (object sender, RoutedEventArgs e)
        {
            /*TextBox block = sender as TextBox;
            Grid grid = block.Parent as Grid;

            foreach (UIElement elem in grid.Children) {
                if (elem is TextBlock)
                    (elem as TextBlock).Visibility = System.Windows.Visibility.Visible;
                else if (elem is TextBox)
                    (elem as TextBox).Visibility = System.Windows.Visibility.Collapsed;
            }*/
        }
    }

    [ValueConversion(typeof(LayerType), typeof(string))]
    public class LayerTypeToIconFilenameConverter : IValueConverter
    {
        private static string IconPath = "/Treefrog.V2;component/Images/16/";

        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((LayerType)value) {
                case LayerType.Generic:
                    return IconPath + "selection.png";
                case LayerType.Object:
                    return IconPath + "game.png";
                case LayerType.Tile:
                    return IconPath + "grid.png";
                default:
                    return null;
            }
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class VisibilityToIconFilenameConverter : IValueConverter
    {
        private static string IconPath = "/Treefrog.V2;component/Images/16/";

        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return IconPath + "eye.png";
            else
                return IconPath + "cross.png";
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class VisibilityToTooltipConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return "Layer is visible";
            else
                return "Layer is hidden";
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
