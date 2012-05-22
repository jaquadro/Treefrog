using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using Treefrog.Framework.Model;
using Treefrog.Controls.Xna;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Treefrog.Framework;
using System.IO;
using Treefrog.ViewModel.Menu;
using Treefrog.ViewModel.ToolBars;

using TextureResource = Treefrog.Framework.Imaging.TextureResource;

namespace Treefrog.ViewModel
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

            _tilePool.TileAdded += HandleTileAdded;
            _tilePool.TileRemoved -= HandleTileRemoved;
        }

        private void HandleTileAdded (object sender, TileEventArgs e)
        {
            _tiles.Add(new TilePoolItemVM(e.Tile));
        }

        private void HandleTileRemoved (object sender, TileEventArgs e)
        {
            foreach (TilePoolItemVM vm in _tiles) {
                if (vm.Tile.Id == e.Tile.Id) {
                    _tiles.Remove(vm);
                    break;
                }
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
                        PropertyManagerService service = ServiceContainer.Default.GetService<PropertyManagerService>();
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
