using System;
using System.Collections.Generic;
using System.Xml;

namespace Treefrog.Framework.Model
{
    using Support;
    using Treefrog.Framework.Imaging;
    using Treefrog.Framework.Model.Proxy;
    using System.Text;

    public class MultiTileGridLayer : TileGridLayer
    {
        #region Fields

        private TileStack[,] _tiles;

        #endregion

        public MultiTileGridLayer (string name, int tileWidth, int tileHeight, int tilesWide, int tilesHigh)
            : base(name, tileWidth, tileHeight, tilesWide, tilesHigh)
        {
            _tiles = new TileStack[tilesHigh, tilesWide];
        }

        public MultiTileGridLayer (string name, int tileWidth, int tileHeight, Level level)
            : base(name, tileWidth, tileHeight, level)
        {
            _tiles = new TileStack[TilesHigh, TilesWide];
        }

        public MultiTileGridLayer (string name, MultiTileGridLayer layer)
            : base(name, layer)
        {
            _tiles = new TileStack[layer.TilesHigh, layer.TilesWide];

            for (int y = 0; y < layer.TilesHigh; y++) {
                for (int x = 0; x < layer.TilesWide; x++) {
                    if (layer._tiles[y, x] != null) {
                        _tiles[y, x] = layer._tiles[y, x].Clone() as TileStack;
                    }
                }
            }
        }

        public MultiTileGridLayer (LevelX.MultiTileGridLayerX proxy, Level level, Dictionary<int, Guid> tileIndex)
            : base(proxy.Name, proxy.TileWidth, proxy.TileHeight, level)
        {
            _tiles = new TileStack[TilesHigh, TilesWide];

            Opacity = proxy.Opacity;
            IsVisible = proxy.Visible;
            RasterMode = proxy.RasterMode;
            Level = level;

            foreach (var blockProxy in proxy.Blocks)
                ParseTileBlockString(blockProxy.Data);

            if (proxy.Properties != null) {
                foreach (var propertyProxy in proxy.Properties)
                    CustomProperties.Add(Property.FromXmlProxy(propertyProxy));
            }
        }

        public static LevelX.MultiTileGridLayerX ToXmlProxyX (MultiTileGridLayer layer)
        {
            if (layer == null)
                return null;

            const int blockSize = 4;

            int xLimit = (layer.TilesWide + blockSize - 1) / blockSize;
            int yLimit = (layer.TilesHigh + blockSize - 1) / blockSize;

            List<CommonX.TileBlockX> blocks = new List<CommonX.TileBlockX>();
            for (int y = 0; y < yLimit; y++) {
                for (int x = 0; x < xLimit; x++) {
                    string data = layer.BuildTileBlockString(x * blockSize, y * blockSize, blockSize, blockSize);
                    if (!string.IsNullOrEmpty(data)) {
                        blocks.Add(new CommonX.TileBlockX() {
                            X = x * blockSize,
                            Y = y * blockSize,
                            Data = data,
                        });
                    }
                }
            }

            List<CommonX.PropertyX> props = new List<CommonX.PropertyX>();
            foreach (Property prop in layer.CustomProperties)
                props.Add(Property.ToXmlProxyX(prop));

            return new LevelX.MultiTileGridLayerX() {
                Name = layer.Name,
                Opacity = layer.Opacity,
                Visible = layer.IsVisible,
                RasterMode = layer.RasterMode,
                TileWidth = layer.TileWidth,
                TileHeight = layer.TileHeight,
                Blocks = blocks.Count > 0 ? blocks : null,
                Properties = props.Count > 0 ? props : null,
            };
        }

        private string BuildTileBlockString (int xStart, int yStart, int width, int height)
        {
            StringBuilder builder = new StringBuilder();

            int xLimit = Math.Min(xStart + width, TilesWide);
            int yLimit = Math.Min(yStart + height, TilesHigh);

            for (int y = yStart; y < yLimit; y++) {
                for (int x = xStart; x < xLimit; x++) {
                    TileStack stack = this[x, y];
                    if (stack != null) {
                        builder.Append(x + " " + y + " " + stack.Count + " ");
                        foreach (Tile tile in stack)
                            builder.Append(Level.TileIndex[tile.Uid] + " ");
                    }
                }
            }

            if (builder.Length > 0)
                builder.Remove(builder.Length - 1, 1);

            return builder.ToString();
        }

        private void ParseTileBlockString (string blockString)
        {
            string[] tokens = blockString.Split(new char[] { ' ' });

            ITilePoolManager manager = Level.Project.TilePoolManager;

            for (int i = 0; i < tokens.Length; i += 3) {
                if (tokens.Length - i < 3)
                    break;

                int x = int.Parse(tokens[i + 0]);
                int y = int.Parse(tokens[i + 1]);
                int count = int.Parse(tokens[i + 2]);

                if (tokens.Length - i < 3 + count)
                    break;

                for (int j = 0; j < count; j++) {
                    int tileId = int.Parse(tokens[i + 3 + j]);

                    if (Level.TileIndex.ContainsKey(tileId)) {
                        Guid tileUid = Level.TileIndex[tileId];

                        TilePool pool = manager.PoolFromItemKey(tileUid);
                        Tile tile = pool.GetTile(tileUid);

                        AddTile(x, y, tile);
                    }
                }

                i += count;
            }
        }

        public TileStack this[TileCoord location]
        {
            get { return this[location.X, location.Y]; }
            set
            {
                this[location.X, location.Y] = value;
            }
        }

        public TileStack this[int x, int y]
        {
            get
            {
                CheckBoundsFail(x, y);
                return _tiles[YIndex(y), XIndex(x)];
            }

            set
            {
                CheckBoundsFail(x, y);

                int xi = XIndex(x);
                int yi = YIndex(y);

                if (_tiles[yi, xi] != null) {
                    _tiles[yi, xi].Modified -= TileStackModifiedHandler;
                }

                if (value != null) {
                    _tiles[yi, xi] = new TileStack(value);
                    _tiles[yi, xi].Modified += TileStackModifiedHandler;
                }
                else {
                    _tiles[yi, xi] = null;
                }

                OnModified(EventArgs.Empty);
            }
        }

        private void TileStackModifiedHandler (object sender, EventArgs e)
        {
            OnModified(e);
        }

        protected override void ResizeLayer (int newOriginX, int newOriginY, int newTilesWide, int newTilesHigh)
        {
            int originDiffX = newOriginX - TileOriginX;
            int originDiffY = newOriginY - TileOriginY;

            int maxOriginX = Math.Max(newOriginX, TileOriginX);
            int maxOriginY = Math.Max(newOriginY, TileOriginY);
            int minEndX = Math.Min(newOriginX + newTilesWide, TileOriginX + TilesWide);
            int minEndY = Math.Min(newOriginY + newTilesHigh, TileOriginY + TilesHigh);

            int startX = maxOriginX - TileOriginX;
            int startY = maxOriginY - TileOriginY;
            int limitX = minEndX - maxOriginX;
            int limitY = minEndY - maxOriginY;

            TileStack[,] newTiles = new TileStack[newTilesHigh, newTilesWide];

            for (int y = startY; y < limitY; y++) {
                for (int x = startX; x < limitX; x++) {
                    newTiles[y - originDiffY, x - originDiffX] = _tiles[y, x];
                }
            }

            //if (newOriginX < TileOriginX)

            // NEEDS WORK

            /*int xAdj = TileOriginX - newOriginX;
            int yAdj = TileOriginY - newOriginY;

            TileStack[,] newTiles = new TileStack[newTilesHigh, newTilesWide];
            
            int startX = Math.Max(0, xAdj);
            int startY = Math.Max(0, yAdj);
            int copyLimX = Math.Min(TilesWide, newTilesWide);
            int copyLimY = Math.Min(TilesHigh, newTilesHigh);

            for (int y = startY; y < copyLimY; y++) {
                for (int x = startX; x < copyLimX; x++) {
                    newTiles[y + yAdj, x + xAdj] = _tiles[y, x];
                }
            }*/

            _tiles = newTiles;
        }

        public override IEnumerable<LocatedTile> Tiles
        {
            get { return TilesAt(new Rectangle(TileOriginX, TileOriginY, TilesWide, TilesHigh)); }
        }

        public override IEnumerable<LocatedTile> TilesAt (TileCoord location)
        {
            if (!CheckBounds(location.X, location.Y))
                yield break;

            int xi = XIndex(location.X);
            int yi = YIndex(location.Y);

            if (_tiles[yi, xi] == null) {
                yield break;
            }

            foreach (Tile tile in _tiles[yi, xi]) {
                yield return new LocatedTile(tile, location);
            }
        }

        public override IEnumerable<LocatedTile> TilesAt (Rectangle region)
        {
            int xs = Math.Max(region.X, TileOriginX);
            int ys = Math.Max(region.Y, TileOriginY);
            int xe = Math.Min(region.X + region.Width, TilesWide + TileOriginX);
            int ye = Math.Min(region.Y + region.Height, TilesHigh + TileOriginY);

            for (int y = ys; y < ye; y++) {
                for (int x = xs; x < xe; x++) {
                    int xi = XIndex(x);
                    int yi = YIndex(y);

                    if (_tiles[yi, xi] == null) {
                        continue;
                    }

                    foreach (Tile tile in _tiles[yi, xi]) {
                        yield return new LocatedTile(tile, x, y);
                    }
                }
            }
        }

        public IEnumerable<LocatedTileStack> TileStacks
        {
            get { return TileStacksAt(new Rectangle(TileOriginX, TileOriginY, TilesWide, TilesHigh)); }
        }

        public TileStack TileStacksAt (TileCoord location)
        {
            if (!CheckBounds(location.X, location.Y))
                return null;

            return _tiles[YIndex(location.Y), XIndex(location.X)];
        }

        public IEnumerable<LocatedTileStack> TileStacksAt (Rectangle region)
        {
            int xs = Math.Max(region.X, TileOriginX);
            int ys = Math.Max(region.Y, TileOriginY);
            int w = Math.Min(region.Width, TilesWide - region.X);
            int h = Math.Min(region.Height, TilesHigh - region.Y);

            for (int y = ys; y < ys + h; y++) {
                for (int x = xs; x < xs + w; x++) {
                    int xi = XIndex(x);
                    int yi = YIndex(y);

                    if (_tiles[yi, xi] != null) {
                        yield return new LocatedTileStack(_tiles[yi, xi], x, y);
                    }
                }
            }
        }

        public void AddTileStack (int x, int y, TileStack stack)
        {
            if (stack != null) {
                foreach (Tile t in stack) {
                    AddTile(x, y, t);
                }
            }
        }

        protected override void AddTileImpl (int x, int y, Tile tile)
        {
            int xi = XIndex(x);
            int yi = YIndex(y);

            if (_tiles[yi, xi] == null) {
                _tiles[yi, xi] = new TileStack();
                _tiles[yi, xi].Modified += TileStackModifiedHandler;
            }

            _tiles[yi, xi].Remove(tile);
            _tiles[yi, xi].Add(tile);
        }

        protected override void RemoveTileImpl (int x, int y, Tile tile)
        {
            int xi = XIndex(x);
            int yi = YIndex(y);

            if (_tiles[yi, xi] != null) {
                _tiles[yi, xi].Remove(tile);
            }
        }

        protected override void ClearTileImpl (int x, int y)
        {
            int xi = XIndex(x);
            int yi = YIndex(y);

            if (_tiles[yi, xi] != null) {
                _tiles[yi, xi].Clear();
            }
        }

        private int XIndex (int x)
        {
            return x - TileOriginX;
        }

        private int YIndex (int y)
        {
            return y - TileOriginY;
        }

        #region ICloneable Members

        public override object Clone ()
        {
            return new MultiTileGridLayer(Name, this);
        }

        #endregion
    }
}
