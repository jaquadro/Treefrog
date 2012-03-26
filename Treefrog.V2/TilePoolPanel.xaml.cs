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
using Treefrog.V2.Controls.Xna;
using Treefrog.V2.ViewModel;

namespace Treefrog.V2
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

        private void GraphicsDeviceControl_LoadContent (object sender, Controls.Xna.GraphicsDeviceEventArgs e)
        {
            GraphicsDeviceControl gdc = sender as GraphicsDeviceControl;
            TilePoolCollectionVM vm = gdc.DataContext as TilePoolCollectionVM;

            if (vm != null)
                vm.InitializePool.Execute(e);
        }
    }
}
