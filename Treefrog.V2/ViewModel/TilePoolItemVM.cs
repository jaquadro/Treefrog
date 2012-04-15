using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Treefrog.Framework.Model;
using Treefrog.Framework.Imaging;

namespace Treefrog.V2.ViewModel
{
    public class TilePoolItemVM : ViewModelBase
    {
        private Tile _tile;
        private TextureResource _source;

        public TilePoolItemVM (Tile tile)
        {
            _tile = tile;

            _source = _tile.Pool.GetTileTexture(_tile.Id);
        }

        public Tile Tile
        {
            get { return _tile; }
        }

        public int Id
        {
            get { return _tile.Id; }
        }

        public TextureResource ImageSource
        {
            get { return _source; }
        }

        public int TileHeight
        {
            get { return _tile.Height; }
        }

        public int TileWidth
        {
            get { return _tile.Width; }
        }
    }
}
