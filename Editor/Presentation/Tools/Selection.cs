using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Framework.Model.Support;
using Amphibian.Drawing;

namespace Treefrog.Presentation.Tools
{
    public class TileSelection : IEnumerable<LocatedTileStack>
    {
        private static Color _defaultBrushColor = new Color(.3f, .7f, 1.0f, .5f);

        private TileCoord _origin;
        private Rectangle _bounds;
        private Dictionary<TileCoord, TileStack> _tiles;
        private int _tileWidth;
        private int _tileHeight;

        private Brush _selectBrush;

        public TileSelection (int tileWidth, int tileHeight)
        {
            _tiles = new Dictionary<TileCoord, TileStack>();
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
        }

        public Rectangle Bounds
        {
            get { return _bounds; }
        }

        public Brush Brush
        {
            get { return _selectBrush; }
            set { _selectBrush = value; }
        }

        public virtual TileCoord Origin
        {
            get { return new TileCoord(_origin.X, _origin.Y); }
            protected set { _origin = value; }
        }

        protected virtual Color DefaultBrushColor
        {
            get { return _defaultBrushColor; }
        }

        public void AddTiles (TileCoord coord, TileStack tiles)
        {
            _tiles[coord] = tiles;
            UpdateBounds(coord);
        }

        public void AddTiles (TileCoord start, TileCoord end, MultiTileGridLayer layer)
        {
            int startx = Math.Max(Math.Min(start.X, end.X), 0);
            int starty = Math.Max(Math.Min(start.Y, end.Y), 0);
            int endx = Math.Min(Math.Max(start.X, end.X), layer.TilesWide - 1);
            int endy = Math.Min(Math.Max(start.Y, end.Y), layer.TilesHigh - 1);

            for (int y = starty; y <= endy; y++) {
                for (int x = startx; x <= endx; x++) {
                    AddTiles(new TileCoord(x, y), layer[x, y]);
                    UpdateBounds(new TileCoord(x, y));
                }
            }
        }

        public void RemoveTiles (TileCoord coord)
        {
            _tiles.Remove(coord);
        }

        public void RemoveTiles (TileCoord start, TileCoord end)
        {
            int startx = Math.Max(Math.Min(start.X, end.X), 0);
            int starty = Math.Max(Math.Min(start.Y, end.Y), 0);
            int endx = Math.Max(start.X, end.X);
            int endy = Math.Max(start.Y, end.Y);

            for (int y = starty; y <= endy; y++) {
                for (int x = startx; x <= endx; x++) {
                    RemoveTiles(new TileCoord(x, y));
                }
            }
        }

        public void DrawSelection (SpriteBatch spriteBatch, float zoom)
        {
            if (_selectBrush == null) {
                _selectBrush = new SolidColorBrush(spriteBatch.GraphicsDevice, DefaultBrushColor);
            }

            foreach (TileCoord coord in _tiles.Keys) {
                Rectangle rect = new Rectangle(
                    (int)((Origin.X + coord.X) * _tileWidth * zoom), 
                    (int)((Origin.Y + coord.Y) * _tileHeight * zoom), 
                    (int)(_tileWidth * zoom), 
                    (int)(_tileHeight * zoom)
                    );
                Draw2D.FillRectangle(spriteBatch, rect, _selectBrush);
            }
        }

        public void DrawTiles (SpriteBatch spriteBatch, float zoom)
        {
            foreach (KeyValuePair<TileCoord, TileStack> kv in _tiles) {
                if (kv.Value == null)
                    continue;
                foreach (Tile tile in kv.Value) {
                    if (tile != null) {
                        Rectangle rect = new Rectangle(
                            (int)((Origin.X + kv.Key.X) * _tileWidth * zoom),
                            (int)((Origin.Y + kv.Key.Y) * _tileHeight * zoom),
                            (int)(_tileWidth * zoom),
                            (int)(_tileHeight * zoom)
                            );
                        tile.Draw(spriteBatch, rect);
                    }
                }
            }
        }

        public bool Contains (TileCoord coord)
        {
            return _tiles.ContainsKey(new TileCoord(coord.X - Origin.X, coord.Y - Origin.Y));
        }

        public FloatingTileSelection CreateFloatingSelection ()
        {
            FloatingTileSelection fs = new FloatingTileSelection(_tileWidth, _tileHeight);
            foreach (LocatedTileStack t in this) {
                fs.AddTiles(t.Location, t.Stack);
            }
            fs.Origin = Origin;

            return fs;
        }

        private void UpdateBounds (TileCoord coord)
        {
            _bounds.X = Math.Min(_bounds.X, coord.X);
            _bounds.Width = Math.Max(_bounds.Width, coord.X - _bounds.X);
            _bounds.Y = Math.Min(_bounds.Y, coord.Y);
            _bounds.Height = Math.Max(_bounds.Height, coord.Y - _bounds.Y);
        }

        #region IEnumerable<LocatedTileStack> Members

        public IEnumerator<LocatedTileStack> GetEnumerator ()
        {
            foreach (KeyValuePair<TileCoord, TileStack> kv in _tiles) {
                yield return new LocatedTileStack(kv.Value, kv.Key);
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return GetEnumerator();
        }

        #endregion
    }

    public class FloatingTileSelection : TileSelection
    {
        private static Color _defaultBrushColor = new Color(.5f, .3f, 1.0f, .5f);

        private TileCoord _dragOrigin;
        private TileCoord _dragDiff;

        internal FloatingTileSelection (int tileWidth, int tileHeight)
            : base(tileWidth, tileHeight)
        {
        }

        public override TileCoord Origin
        {
            get { return new TileCoord(base.Origin.X + _dragDiff.X, base.Origin.Y + _dragDiff.Y); }
        }

        protected override Color DefaultBrushColor
        {
            get { return _defaultBrushColor; }
        }

        public void StartMove (TileCoord location)
        {
            _dragOrigin = location;
        }

        public void Move (TileCoord location)
        {
            _dragDiff = new TileCoord(location.X - _dragOrigin.X, location.Y - _dragOrigin.Y);
        }

        public void EndMove (TileCoord location)
        {
            Move(location);
            Origin = new TileCoord(base.Origin.X + _dragDiff.X, base.Origin.Y + _dragDiff.Y);
            _dragDiff = new TileCoord(0, 0);
        }
    }
}
