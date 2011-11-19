using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Treefrog.Framework.Model
{
    using Support;

    public abstract class TileGridLayer : TileLayer
    {
        #region Fields

        private int _tilesWide;
        private int _tilesHigh;

        #endregion

        #region Constructors

        protected TileGridLayer (string name, int tileWidth, int tileHeight, int tilesWide, int tilesHigh)
            : base(name, tileWidth, tileHeight)
        {
            _tilesWide = tilesWide;
            _tilesHigh = tilesHigh;
        }

        protected TileGridLayer (string name, TileGridLayer layer)
            : base(name, layer)
        {
            _tilesHigh = layer._tilesHigh;
            _tilesWide = layer._tilesWide;
        }

        #endregion

        #region Properties

        public int LayerHeight
        {
            get { return _tilesHigh; }
        }

        public int LayerWidth
        {
            get { return _tilesWide; }
        }

        #endregion

        #region Events

        public event EventHandler LayerSizeChanged;

        #endregion

        #region Event Dispatchers

        protected void OnLayerSizeChanged (EventArgs e)
        {
            if (LayerSizeChanged != null) {
                LayerSizeChanged(this, e);
            }
            OnModified(EventArgs.Empty);
        }

        #endregion

        #region Public API

        public void AddTile (int x, int y, Tile tile)
        {
            CheckBoundsFail(x, y);
            CheckTileFail(tile);

            AddTileImpl(x, y, tile);
        }

        public void RemoveTile (int x, int y, Tile tile)
        {
            CheckBoundsFail(x, y);

            RemoveTileImpl(x, y, tile);
        }

        public void ClearTile (int x, int y)
        {
            CheckBoundsFail(x, y);

            ClearTileImpl(x, y);
        }

        public abstract IEnumerable<LocatedTile> Tiles { get; }

        public abstract IEnumerable<LocatedTile> TilesAt (TileCoord location);

        public abstract IEnumerable<LocatedTile> TilesAt (Rectangle region);

        #endregion

        #region Virtual Backing API

        protected abstract void AddTileImpl (int x, int y, Tile tile);

        protected abstract void RemoveTileImpl (int x, int y, Tile tile);

        protected abstract void ClearTileImpl (int x, int y);

        #endregion

        #region Checking Code

        protected void CheckTileFail (Tile tile)
        {
            if (!CheckTile(tile)) {
                throw new ArgumentException(String.Format("Tried to add tile with dimenions ({0}, {1}), layer expects tile dimensions ({2}, {3})",
                    new string[] { tile.Width.ToString(), tile.Height.ToString(), TileWidth.ToString(), TileHeight.ToString() }));
            }
        }

        protected void CheckBoundsFail (int x, int y)
        {
            if (!CheckBounds(x, y)) {
                throw new ArgumentOutOfRangeException(String.Format("Tried to add tile at ({0}, {1}), which is outside of layer dimensions ({2}, {3})",
                    new string[] { x.ToString(), y.ToString(), LayerWidth.ToString(), LayerHeight.ToString() }));
            }
        }

        protected bool CheckTile (Tile tile)
        {
            return tile.Width == TileWidth && tile.Height == TileHeight;
        }

        protected bool CheckBounds (int x, int y)
        {
            return x >= 0 &&
                y >= 0 &&
                x < LayerWidth &&
                y < LayerHeight;
        }

        #endregion
    }
}
