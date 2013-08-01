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
        private int _tileOriginX;
        private int _tileOriginY;
        private int _tilesWide;
        private int _tilesHigh;

        protected TileGridLayer (string name, int tileWidth, int tileHeight, int tilesWide, int tilesHigh)
            : base(name, tileWidth, tileHeight)
        {
            _tileOriginX = 0;
            _tileOriginY = 0;
            _tilesWide = tilesWide;
            _tilesHigh = tilesHigh;
        }

        protected TileGridLayer (string name, int tileWidth, int tileHeight, Level level)
            : base(name, tileWidth, tileHeight)
        {
            Level = level;

            _tileOriginX = (int)Math.Floor(level.OriginX * 1.0 / tileWidth);
            _tileOriginY = (int)Math.Floor(level.OriginY * 1.0 / tileHeight);

            int diffOriginX = level.OriginX - (_tileOriginX * tileWidth);
            int diffOriginY = level.OriginY - (_tileOriginY * tileHeight);

            _tilesWide = (int)Math.Ceiling((level.OriginX + level.Width + diffOriginX) * 1.0 / tileWidth) - _tileOriginX;
            _tilesHigh = (int)Math.Ceiling((level.OriginY + level.Height + diffOriginY) * 1.0 / tileHeight) - _tileOriginY;
        }

        protected TileGridLayer (string name, TileGridLayer layer)
            : base(name, layer)
        {
            _tileOriginX = layer._tileOriginX;
            _tileOriginY = layer._tileOriginY;
            _tilesHigh = layer._tilesHigh;
            _tilesWide = layer._tilesWide;
        }

        public int TileOriginX
        {
            get { return _tileOriginX; }
        }

        public int TileOriginY
        {
            get { return _tileOriginY; }
        }

        public int TilesHigh
        {
            get { return _tilesHigh; }
        }

        public int TilesWide
        {
            get { return _tilesWide; }
        }

        public bool CanAddTile (int x, int y, Tile tile)
        {
            return CheckTile(tile) && CheckBounds(x, y);
        }

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

        public void RemoveAllMatchingTiles (Tile tile)
        {
            for (int y = TileOriginY; y < TileOriginY + TilesHigh; y++) {
                for (int x = TileOriginX; x < TileOriginX + TilesWide; x++) {
                    RemoveTile(x, y, tile);
                }
            }
        }

        public abstract IEnumerable<LocatedTile> Tiles { get; }

        public abstract IEnumerable<LocatedTile> TilesAt (TileCoord location);

        public abstract IEnumerable<LocatedTile> TilesAt (Rectangle region);

        public override int LayerOriginX
        {
            get { return _tileOriginX * TileWidth; }
        }

        public override int LayerOriginY
        {
            get { return _tileOriginY * TileHeight; }
        }

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

        public override void RequestNewSize (int originX, int originY, int pixelsWide, int pixelsHigh)
        {
            if (pixelsWide <= 0 || pixelsHigh <= 0) {
                throw new ArgumentException("New layer dimensions must be greater than 0.");
            }

            int tileX = (int)Math.Floor(originX * 1.0 / TileWidth);
            int tileY = (int)Math.Floor(originY * 1.0 / TileHeight);

            int tilesW = (int)Math.Ceiling((originX + pixelsWide) * 1.0 / TileWidth) - tileX;
            int tilesH = (int)Math.Ceiling((originY + pixelsHigh) * 1.0 / TileHeight) - tileY;

            if (tileX != _tileOriginX || tileY != _tileOriginY || tilesW != _tilesWide || tilesH != _tilesHigh) {
                ResizeLayer(tileX, tileY, tilesW, tilesH);

                _tileOriginX = tileX;
                _tileOriginY = tileY;
                _tilesWide = tilesW;
                _tilesHigh = tilesH;

                OnLayerSizeChanged(EventArgs.Empty);
            }
        }

        public bool InRange (int x, int y)
        {
            return CheckBounds(x, y);
        }

        public bool InRange (TileCoord coord)
        {
            return CheckBounds(coord.X, coord.Y);
        }

        public bool InRange (LocatedTile tile)
        {
            return CheckBounds(tile.X, tile.Y);
        }

        protected abstract void AddTileImpl (int x, int y, Tile tile);

        protected abstract void RemoveTileImpl (int x, int y, Tile tile);

        protected abstract void ClearTileImpl (int x, int y);

        protected abstract void ResizeLayer (int newOriginX, int newOriginY, int newTilesWide, int newTilesHigh);

        public event EventHandler<LocatedTileEventArgs> TileAdding;

        public event EventHandler<LocatedTileEventArgs> TileRemoving;

        public event EventHandler<LocatedTileEventArgs> TileClearing;

        public event EventHandler<LocatedTileEventArgs> TileAdded;

        public event EventHandler<LocatedTileEventArgs> TileRemoved;

        public event EventHandler<LocatedTileEventArgs> TileCleared;

        protected virtual void OnTileAdding (LocatedTileEventArgs e)
        {
            var ev = TileAdding;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnTileRemoving (LocatedTileEventArgs e)
        {
            var ev = TileRemoving;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnTileClearing (LocatedTileEventArgs e)
        {
            var ev = TileClearing;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnTileAdded (LocatedTileEventArgs e)
        {
            var ev = TileAdded;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnTileRemoved (LocatedTileEventArgs e)
        {
            var ev = TileRemoved;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnTileCleared (LocatedTileEventArgs e)
        {
            var ev = TileCleared;
            if (ev != null)
                ev(this, e);
        }

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
                throw new ArgumentOutOfRangeException(String.Format("Tried to add tile at ({0}, {1}), which is outside of layer dimensions ({2}, {3}),({4}, {5})",
                    new string[] { x.ToString(), y.ToString(), 
                        TileOriginX.ToString(), TileOriginY.ToString(),
                        (TilesWide - TileOriginX).ToString(), (TilesHigh - TileOriginY).ToString() }));
            }
        }

        protected bool CheckTile (Tile tile)
        {
            return tile.Width == TileWidth && tile.Height == TileHeight;
        }

        protected bool CheckBounds (int x, int y)
        {
            return x >= TileOriginX
                && y >= TileOriginY
                && x < (TilesWide + TileOriginX)
                && y < (TilesHigh + TileOriginY);
        }
    }
}
