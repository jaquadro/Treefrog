using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Editor.Model
{
    public abstract class Layer
    {
        private float _opacity;
        private bool _visible;
    }

    public abstract class TileLayer : Layer
    {
        #region Fields

        private int _tileWidth;
        private int _tileHeight;

        private int _tilesWide;
        private int _tilesHigh;

        #endregion

        #region Constructors

        protected TileLayer (int tileWidth, int tileHeight, int tilesWide, int tilesHigh)
        {
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;

            _tilesWide = tilesWide;
            _tilesHigh = tilesHigh;
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

        public int TileHeight
        {
            get { return _tileHeight; }
        }

        public int TileWidth
        {
            get { return _tileWidth; }
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

        private void CheckTileFail (Tile tile)
        {
            if (!CheckTile(tile)) {
                throw new ArgumentException(String.Format("Tried to add tile with dimenions ({0}, {1}), layer expects tile dimensions ({2}, {3})",
                    new string[] { tile.Width.ToString(), tile.Height.ToString(), TileWidth.ToString(), TileHeight.ToString() }));
            }
        }

        private void CheckBoundsFail (int x, int y)
        {
            if (!CheckBounds(x, y)) {
                throw new ArgumentOutOfRangeException(String.Format("Tried to add tile at ({0}, {1}), which is outside of layer dimensions ({2}, {3})",
                    new string[] { x.ToString(), y.ToString(), LayerWidth.ToString(), LayerHeight.ToString() }));
            }
        }

        private bool CheckTile (Tile tile)
        {
            return tile.Width == TileWidth && tile.Height == TileHeight;
        }

        private bool CheckBounds (int x, int y)
        {
            return x >= 0 &&
                y >= 0 &&
                x < LayerWidth &&
                y < LayerHeight;
        }

        #endregion
    }

    public class MultiTileLayer : TileLayer
    {
        #region Fields

        private TileStack[,] _tiles;

        #endregion

        #region Constructors

        public MultiTileLayer (int tileWidth, int tileHeight, int tilesWide, int tilesHigh)
            : base(tileWidth, tileHeight, tilesWide, tilesHigh)
        {
            _tiles = new TileStack[tilesHigh, tilesWide];
        }

        #endregion

        #region Properties

        #endregion

        public override IEnumerable<LocatedTile> Tiles
        {
            get { return TilesAt(new Rectangle(0, 0, LayerWidth, LayerHeight)); }
        }

        public override IEnumerable<LocatedTile> TilesAt (TileCoord location)
        {
            if (_tiles[location.Y, location.X] == null) {
                yield break;
            }

            foreach (Tile tile in _tiles[location.Y, location.X].Tiles) {
                yield return new LocatedTile(tile, location);
            }
        }

        public override IEnumerable<LocatedTile> TilesAt (Rectangle region)
        {
            int xs = Math.Max(region.X, 0);
            int ys = Math.Max(region.Y, 0);
            int w = Math.Min(region.X, LayerWidth);
            int h = Math.Min(region.Y, LayerHeight);

            for (int y = ys; y < ys + h; y++) {
                for (int x = xs; x < xs + w; x++) {
                    foreach (Tile tile in _tiles[y, x].Tiles) {
                        yield return new LocatedTile(tile, x, y);
                    }
                }
            }
        }

        public IEnumerable<LocatedTileStack> TileStacks
        {
            get { return TileStacksAt(new Rectangle(0, 0, LayerWidth, LayerHeight)); }
        }

        public IEnumerable<LocatedTileStack> TileStacksAt (Rectangle region)
        {
            int xs = Math.Max(region.X, 0);
            int ys = Math.Max(region.Y, 0);
            int w = Math.Min(region.X, LayerWidth);
            int h = Math.Min(region.Y, LayerHeight);

            for (int y = ys; y < ys + h; y++) {
                for (int x = xs; x < xs + w; x++) {
                    if (_tiles[y, x] != null) {
                        yield return new LocatedTileStack(_tiles[y, x], x, y);
                    }
                }
            }
        }

        protected override void AddTileImpl (int x, int y, Tile tile)
        {
            _tiles[y, x].Remove(tile);
            _tiles[y, x].Add(tile);
        }

        protected override void RemoveTileImpl (int x, int y, Tile tile)
        {
            _tiles[y, x].Remove(tile);
        }

        protected override void ClearTileImpl (int x, int y)
        {
            _tiles[y, x].Clear();
        }
    }
}
