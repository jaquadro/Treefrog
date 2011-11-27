using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Framework;
using Treefrog.Framework.Model;

namespace Treefrog.Presentation.Layers
{
    public class DrawLayerEventArgs : EventArgs
    {
        public SpriteBatch SpriteBatch { get; private set; }

        public DrawLayerEventArgs (SpriteBatch spriteBatch)
        {
            SpriteBatch = spriteBatch;
        }
    }

    public class TileMouseEventArgs : MouseEventArgs
    {
        public TileCoord TileLocation { get; private set; }
        public Tile Tile { get; private set; }

        public TileMouseEventArgs (MouseEventArgs e, TileCoord coord)
            : base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
        {
            TileLocation = coord;
        }

        public TileMouseEventArgs (MouseEventArgs e, TileCoord coord, Tile tile)
            : this(e, coord)
        {
            Tile = tile;
        }
    }

    public class TileEventArgs : EventArgs
    {
        private TileCoord _coord;
        private Tile _tile;

        public TileEventArgs (TileCoord coord)
        {
            _coord = coord;
        }

        public TileEventArgs (TileCoord coord, Tile tile)
            : this(coord)
        {
            _tile = tile;
        }

        public TileCoord Location
        {
            get { return _coord; }
        }

        public Tile Tile
        {
            get { return _tile; }
        }
    }

    public class TileSelectEventArgs : EventArgs
    {
        private Dictionary<TileCoord, Tile> _tiles;

        public TileSelectEventArgs (Dictionary<TileCoord, Tile> tiles)
        {
            _tiles = new Dictionary<TileCoord, Tile>();

            // Normalize
            int minx = int.MaxValue;
            int miny = int.MaxValue;

            foreach (TileCoord tc in tiles.Keys) {
                minx = Math.Min(minx, tc.X);
                miny = Math.Min(miny, tc.Y);
            }

            foreach (KeyValuePair<TileCoord, Tile> kv in tiles) {
                _tiles.Add(new TileCoord(kv.Key.X - minx, kv.Key.Y - miny), kv.Value);
            }
        }

        public IEnumerable<KeyValuePair<TileCoord, Tile>> Tiles
        {
            get
            {
                foreach (KeyValuePair<TileCoord, Tile> kv in _tiles) {
                    yield return kv;
                }
            }
        }
    }
}
