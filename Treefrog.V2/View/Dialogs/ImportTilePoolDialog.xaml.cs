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
using System.Windows.Shapes;
using Treefrog.ViewModel.Dialogs;

namespace Treefrog.View.Dialogs
{
    /// <summary>
    /// Interaction logic for ImportTilePoolDialog.xaml
    /// </summary>
    public partial class ImportTilePoolDialog : Window
    {
        public ImportTilePoolDialog ()
        {
            InitializeComponent(); 
            this.DataContextChanged += DataContextChangedHandler;
        }

        private void DataContextChangedHandler (object sender, DependencyPropertyChangedEventArgs e)
        {
            IDialogViewModel d = e.NewValue as IDialogViewModel;
            if (d == null)
                return;

            d.CloseRequested += CloseRequestedHandler;
        }

        private void CloseRequestedHandler (object sender, EventArgs e)
        {
            this.DialogResult = true;
        }

        private void Image_MouseDown (object sender, MouseButtonEventArgs e)
        {
            Image image = sender as Image;
            if (image != null) {
                Point coord = e.GetPosition(image);

                try {
                    BitmapSource source = image.Source as BitmapSource;
                    CroppedBitmap bmp = new CroppedBitmap(source, new Int32Rect((int)coord.X, (int)coord.Y, 1, 1));

                    byte[] pixels = new byte[4];
                    bmp.CopyPixels(pixels, 4, 0);

                    TransColor.SetCurrentValue(Rectangle.FillProperty, new SolidColorBrush(Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0])));
                }
                catch (Exception) { }
            }
        }
    }

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
