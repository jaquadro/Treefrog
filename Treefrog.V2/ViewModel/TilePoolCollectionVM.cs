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
using Treefrog.V2.ViewModel.Layers;

using TextureResource = Treefrog.Framework.Imaging.TextureResource;

namespace Treefrog.V2.ViewModel
{
    public interface TilePoolManagerService
    {
        TilePoolVM ActiveTilePool { get; }
        TilePoolItemVM ActiveTile { get; }
    }

    public class TilePoolCollectionVM : ViewModelBase, TilePoolManagerService
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

            if (pools.Count > 0)
                ActiveTilePool = _tilePools.First();
        }

        public ObservableCollection<TilePoolVM> TilePools
        {
            get { return _tilePools; }
        }

        private TilePoolVM _selected;

        public TilePoolVM ActiveTilePool
        {
            get { return _selected; }
            set
            {
                if (_selected != value) {
                    _selected = value;
                    RaisePropertyChanged("ActiveTilePool");
                    RaisePropertyChanged("SelectedBitmapSource");
                }
            }
        }

        public TilePoolItemVM ActiveTile
        {
            get 
            {
                TilePoolVM pool = ActiveTilePool;
                if (pool != null)
                    return pool.SelectedTile;
                return null;
            }
        }

        public TextureResource SelectedBitmapSource
        {
            get
            {
                if (_selected == null)
                    return null;
                return _selected.BitmapSource;
            }
        }
    }
}
