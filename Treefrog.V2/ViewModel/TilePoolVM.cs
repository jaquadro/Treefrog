using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using Treefrog.Framework.Model;
using Treefrog.V2.Controls.Xna;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Treefrog.Framework;
using System.IO;
using Treefrog.V2.ViewModel.Menu;
using Treefrog.V2.ViewModel.ToolBars;

using TextureResource = Treefrog.Framework.Imaging.TextureResource;

namespace Treefrog.V2.ViewModel
{
    public class TilePoolVM : ViewModelBase
    {
        private TilePool _tilePool;

        private ObservableCollection<TilePoolItemVM> _tiles;

        public TilePoolVM (TilePool tilePool)
        {
            _tilePool = tilePool;
            _tiles = new ObservableCollection<TilePoolItemVM>();

            foreach (Tile tile in _tilePool) {
                _tiles.Add(new TilePoolItemVM(tile));
            }
        }

        public TilePool TilePool
        {
            get { return _tilePool; }
        }

        public ObservableCollection<TilePoolItemVM> Tiles
        {
            get { return _tiles; }
        }

        private TilePoolItemVM _selected;

        public TilePoolItemVM SelectedTile
        {
            get { return _selected; }
            set
            {
                if (_selected != value) {
                    _selected = value;
                    RaisePropertyChanged("SelectedTile");

                    if (_selected != null) {
                        PropertyManagerService service = GalaSoft.MvvmLight.ServiceContainer.Default.GetService<PropertyManagerService>();
                        service.ActiveProvider = _selected.Tile;
                    }
                }
            }
        }

        public string Name
        {
            get { return _tilePool.Name; }
        }

        public TextureResource BitmapSource
        {
            get { return _tilePool.TileSource; }
        }
    }
}
