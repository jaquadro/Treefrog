using System;
using System.Collections.Generic;
using System.Xml;

namespace Treefrog.Framework.Model
{
    using Support;
    using Treefrog.Framework.Imaging;
    using Treefrog.Framework.Model.Proxy;

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

            foreach (var tileProxy in proxy.Tiles) {
                string[] coords = tileProxy.At.Split(new char[] { ',' });
                string[] ids = tileProxy.Items.Split(new char[] { ',' });

                ITilePoolManager manager = Level.Project.TilePoolManager;

                foreach (string id in ids) {
                    int tileId = Convert.ToInt32(id);
                    
                    Guid tileUid;
                    if (!tileIndex.TryGetValue(tileId, out tileUid))
                        continue;

                    TilePool pool = manager.PoolFromItemKey(tileUid);
                    Tile tile = pool.GetTile(tileUid);

                    AddTile(Convert.ToInt32(coords[0]), Convert.ToInt32(coords[1]), tile);
                }
            }

            if (proxy.Properties != null) {
                foreach (var propertyProxy in proxy.Properties)
                    CustomProperties.Add(Property.FromXmlProxy(propertyProxy));
            }
        }

        public static LevelX.MultiTileGridLayerX ToXmlProxyX (MultiTileGridLayer layer)
        {
            if (layer == null)
                return null;

            List<CommonX.TileStackX> tiles = new List<CommonX.TileStackX>();
            foreach (LocatedTileStack ts in layer.TileStacks) {
                if (ts.Stack != null && ts.Stack.Count > 0) {
                    List<string> ids = new List<string>();
                    foreach (Tile tile in ts.Stack) {
                        if (layer.Level.TileIndex.ContainsValue(tile.Uid))
                            ids.Add(layer.Level.TileIndex[tile.Uid].ToString());
                    }
                    string idSet = String.Join(",", ids.ToArray());

                    tiles.Add(new CommonX.TileStackX() {
                        At = ts.X + "," + ts.Y,
                        Items = idSet,
                    });
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
                Tiles = tiles.Count > 0 ? tiles : null,
                Properties = props.Count > 0 ? props : null,
            };
        }

        #endregion

        #region Properties

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

        #endregion

        #region Event Handlers

        private void TileStackModifiedHandler (object sender, EventArgs e)
        {
            OnModified(e);
        }

        #endregion

        protected override void ResizeLayer (int newOriginX, int newOriginY, int newTilesWide, int newTilesHigh)
        {
            TileStack[,] newTiles = new TileStack[newTilesHigh, newTilesWide];
            int copyLimX = Math.Min(TilesWide, newTilesWide);
            int copyLimY = Math.Min(TilesHigh, newTilesHigh);

            for (int y = 0; y < copyLimY; y++) {
                for (int x = 0; x < copyLimX; x++) {
                    newTiles[y, x] = _tiles[y, x];
                }
            }

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
