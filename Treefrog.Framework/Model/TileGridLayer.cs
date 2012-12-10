using System;
using System.Collections.Generic;

namespace Treefrog.Framework.Model
{
    using Support;
    using Treefrog.Framework.Imaging;

    public class LocatedTileEventArgs : TileEventArgs 
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public LocatedTileEventArgs (Tile tile, int x, int y)
            : base(tile)
        {
            X = x;
            Y = y;
        }
    }

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

        public int TilesHigh
        {
            get { return _tilesHigh; }
        }

        public int TilesWide
        {
            get { return _tilesWide; }
        }

        #endregion

        #region Public API

        public void AddTile (int x, int y, Tile tile)
        {
            CheckBoundsFail(x, y);
            CheckTileFail(tile);

            LocatedTileEventArgs ea = new LocatedTileEventArgs(tile, x, y);
            OnTileAdding(ea);
            AddTileImpl(x, y, tile);
            OnTileAdded(ea);
        }

        public void RemoveTile (int x, int y, Tile tile)
        {
            CheckBoundsFail(x, y);

            LocatedTileEventArgs ea = new LocatedTileEventArgs(tile, x, y);
            OnTileRemoving(ea);
            RemoveTileImpl(x, y, tile);
            OnTileRemoved(ea);
        }

        // NB: Consider changing event to give back the original TileStack
        public void ClearTile (int x, int y)
        {
            CheckBoundsFail(x, y);

            LocatedTileEventArgs ea = new LocatedTileEventArgs(null, x, y);
            OnTileClearing(ea);
            ClearTileImpl(x, y);
            OnTileCleared(ea);
        }

        public abstract IEnumerable<LocatedTile> Tiles { get; }

        public abstract IEnumerable<LocatedTile> TilesAt (TileCoord location);

        public abstract IEnumerable<LocatedTile> TilesAt (Rectangle region);

        public override int LayerHeight
        {
            get { return _tilesHigh * TileHeight; }
        }

        public override int LayerWidth
        {
            get { return _tilesWide * TileWidth; }
        }

        public override bool IsResizable
        {
            get { return true; }
        }

        public override void RequestNewSize (int pixelsWide, int pixelsHigh)
        {
            if (pixelsWide <= 0 || pixelsHigh <= 0) {
                throw new ArgumentException("New layer dimensions must be greater than 0.");
            }

            int tilesW = (pixelsWide + TileWidth - 1) / TileWidth;
            int tilesH = (pixelsHigh + TileHeight - 1) / TileHeight;

            if (tilesW != _tilesWide || tilesH != _tilesHigh) {
                ResizeLayer(tilesW, tilesH);
                OnLayerSizeChanged(EventArgs.Empty);
            }
        }

        #endregion

        #region Virtual Backing API

        protected abstract void AddTileImpl (int x, int y, Tile tile);

        protected abstract void RemoveTileImpl (int x, int y, Tile tile);

        protected abstract void ClearTileImpl (int x, int y);

        protected abstract void ResizeLayer (int newTilesWide, int newTilesHigh);

        #endregion

        public event EventHandler<LocatedTileEventArgs> TileAdding = (s, e) => { };

        public event EventHandler<LocatedTileEventArgs> TileRemoving = (s, e) => { };

        public event EventHandler<LocatedTileEventArgs> TileClearing = (s, e) => { };

        public event EventHandler<LocatedTileEventArgs> TileAdded = (s, e) => { };

        public event EventHandler<LocatedTileEventArgs> TileRemoved = (s, e) => { };

        public event EventHandler<LocatedTileEventArgs> TileCleared = (s, e) => { };

        protected virtual void OnTileAdding (LocatedTileEventArgs e)
        {
            TileAdding(this, e);
        }

        protected virtual void OnTileRemoving (LocatedTileEventArgs e)
        {
            TileRemoving(this, e);
        }

        protected virtual void OnTileClearing (LocatedTileEventArgs e)
        {
            TileClearing(this, e);
        }

        protected virtual void OnTileAdded (LocatedTileEventArgs e)
        {
            TileAdded(this, e);
            OnModified(e);
        }

        protected virtual void OnTileRemoved (LocatedTileEventArgs e)
        {
            TileRemoved(this, e);
            OnModified(e);
        }

        protected virtual void OnTileCleared (LocatedTileEventArgs e)
        {
            TileCleared(this, e);
            OnModified(e);
        }

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
                    new string[] { x.ToString(), y.ToString(), TilesWide.ToString(), TilesHigh.ToString() }));
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
                x < TilesWide &&
                y < TilesHigh;
        }

        #endregion
    }
}
