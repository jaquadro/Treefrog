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
using Treefrog.Controls.Xna;
using Treefrog.ViewModel;

using TextureResource = Treefrog.Framework.Imaging.TextureResource;

namespace Treefrog.View.Panels
{
    /// <summary>
    /// Interaction logic for TilePoolPanel.xaml
    /// </summary>
    public partial class TilePoolPanel : UserControl
    {
        public TilePoolPanel ()
        {
            InitializeComponent();
        }
    }

    [ValueConversion(typeof(TextureResource), typeof(BitmapSource))]
    public class TextureResourceToBitmapSourceConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TextureResource tex = value as TextureResource;
            if (tex == null)
                return null;

            TextureResource texCopy = tex.Crop(tex.Bounds);
            texCopy.Apply(c =>
            {
                return new Treefrog.Framework.Imaging.Color(c.B, c.G, c.R, c.A);
            });

            BitmapSource conv = BitmapSource.Create(texCopy.Width, texCopy.Height, 96, 96, 
                PixelFormats.Bgra32, null, texCopy.RawData, texCopy.ScanlineSize);
            return conv;
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
