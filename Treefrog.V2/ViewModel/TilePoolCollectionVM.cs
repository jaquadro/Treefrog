using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using Treefrog.V2.Controls.Xna;
using Treefrog.Framework.Model;
using System.Windows.Input;
using Treefrog.Framework;
using GalaSoft.MvvmLight.Command;

namespace Treefrog.V2.ViewModel
{
    public class TilePoolCollectionVM : ViewModelBase
    {
        private ObservableCollection<TilePoolVM> _tilePools;

        public TilePoolCollectionVM ()
            : base()
        {
            _tilePools = new ObservableCollection<TilePoolVM>();
        }

        public TilePoolCollectionVM (NamedResourceCollection<TilePool> pools)
            : base()
        {
            _tilePools = new ObservableCollection<TilePoolVM>();

            foreach (TilePool pool in pools) {
                _tilePools.Add(new TilePoolVM(pool));
            }
        }

        public ObservableCollection<TilePoolVM> TilePools
        {
            get { return _tilePools; }
        }

        private void InitializePoolEx (GraphicsDeviceEventArgs e)
        {
            TileRegistry reg = new TileRegistry(e.GraphicsDevice);

            _tilePools.Add(new TilePoolVM(new TilePool("Green Meadows", reg, 20, 20)));
            _tilePools.Add(new TilePoolVM(new TilePool("Orange Magma", reg, 20, 20)));
        }

        public ICommand InitializePool
        {
            get { return new RelayCommand<GraphicsDeviceEventArgs>(InitializePoolEx); }
        }
    }
}
