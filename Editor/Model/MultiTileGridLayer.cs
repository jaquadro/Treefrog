using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml;

namespace Editor.Model
{
    public class MultiTileGridLayer : TileGridLayer
    {
        #region Fields

        private TileStack[,] _tiles;

        #endregion

        #region Constructors

        public MultiTileGridLayer (string name, int tileWidth, int tileHeight, int tilesWide, int tilesHigh)
            : base(name, tileWidth, tileHeight, tilesWide, tilesHigh)
        {
            _tiles = new TileStack[tilesHigh, tilesWide];
        }

        #endregion

        #region Properties

        public TileStack this[TileCoord location]
        {
            get { return this[location.X, location.Y]; }
            set { this[location.X, location.Y] = value; }
        }

        public TileStack this[int x, int y]
        {
            get
            {
                CheckBoundsFail(x, y);
                return _tiles[y, x];
            }

            set
            {
                CheckBoundsFail(x, y);
                _tiles[y, x] = new TileStack(value);
            }
        }

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

            foreach (Tile tile in _tiles[location.Y, location.X]) {
                yield return new LocatedTile(tile, location);
            }
        }

        public override IEnumerable<LocatedTile> TilesAt (Rectangle region)
        {
            int xs = Math.Max(region.X, 0);
            int ys = Math.Max(region.Y, 0);
            int xe = Math.Min(region.X + region.Width, LayerWidth);
            int ye = Math.Min(region.Y + region.Height, LayerHeight);

            for (int y = ys; y < ye; y++) {
                for (int x = xs; x < xe; x++) {
                    if (_tiles[y, x] == null) {
                        continue;
                    }

                    foreach (Tile tile in _tiles[y, x]) {
                        yield return new LocatedTile(tile, x, y);
                    }
                }
            }
        }

        public IEnumerable<LocatedTileStack> TileStacks
        {
            get { return TileStacksAt(new Rectangle(0, 0, LayerWidth, LayerHeight)); }
        }

        public TileStack TileStacksAt (TileCoord location)
        {
            return _tiles[location.Y, location.X];
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
            if (_tiles[y, x] == null) {
                _tiles[y, x] = new TileStack();
            }

            _tiles[y, x].Remove(tile);
            _tiles[y, x].Add(tile);
        }

        protected override void RemoveTileImpl (int x, int y, Tile tile)
        {
            if (_tiles[y, x] != null) {
                _tiles[y, x].Remove(tile);
            }
        }

        protected override void ClearTileImpl (int x, int y)
        {
            if (_tiles[y, x] != null) {
                _tiles[y, x].Clear();
            }
        }

        public override void WriteXml (XmlWriter writer)
        {
            // <layer name="" type="multi">
            writer.WriteStartElement("layer");
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("type", "tilemulti");

            writer.WriteEndElement();
        }
    }
}
