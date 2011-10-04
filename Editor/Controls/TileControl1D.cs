using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Editor.Model;
using Microsoft.Xna.Framework;

namespace Editor
{
    public class TileControl1D : TileControl
    {
        private ITileSource1D _tileSource;

        private HashSet<int> _selectedTiles;

        public TileControl1D ()
            : base()
        {
            _selectedTiles = new HashSet<int>();
        }

        public override ITileSource TileSource
        {
            get { return base.TileSource; }
            set
            {
                if (value != null && value as ITileSource1D == null) {
                    throw new ArgumentException("Expected an ITileSource1D");
                }

                _tileSource = value as ITileSource1D;
                if (_tileSource != null) {
                    _canScrollH = false;
                }

                base.TileSource = value;
            }
        }

        protected override IEnumerable<TileCoord> SelectedTileLocations
        {
            get
            {
                int tilesWide = Width / _tileSource.TileWidth;

                foreach (int tid in _selectedTiles) {
                    int x = tid % tilesWide;
                    int y = tid / tilesWide;

                    yield return new TileCoord(x, y);
                }
            }
        }

        protected override IEnumerable<KeyValuePair<TileCoord, Tile>> SelectedTiles
        {
            get
            {
                int tilesWide = Width / _tileSource.TileWidth;

                foreach (int tid in _selectedTiles) {
                    int x = tid % tilesWide;
                    int y = tid / tilesWide;

                    if (tid < 0 || tid >= _tileSource.Capacity)
                        continue;

                    yield return new KeyValuePair<TileCoord, Tile>(new TileCoord(x, y), _tileSource[tid]);
                }
            }
        }

        protected override void DrawTiles (Vector2 offset, Rectangle region)
        {
            int tilesWide = Width / _tileSource.TileWidth;

            int pointX = 0;
            int pointY = 0;

            foreach (Tile tile in _tileSource) {
                Rectangle dest = new Rectangle(
                    pointX * (int)(_tileSource.TileWidth * Zoom),
                    pointY * (int)(_tileSource.TileHeight * Zoom),
                    (int)(_tileSource.TileWidth * Zoom),
                    (int)(_tileSource.TileHeight * Zoom));
                tile.Draw(_spriteBatch, dest);

                if (++pointX >= tilesWide) {
                    pointX = 0;
                    pointY++;
                }
            }
        }

        public override System.Drawing.Rectangle VirtualSize
        {
            get
            {
                if (_tileSource == null) {
                    return new System.Drawing.Rectangle(0, 0, 0, 0);
                }

                int tilesWide = Width / _tileSource.TileWidth;

                int width = tilesWide * _tileSource.TileWidth;
                int height = ((_tileSource.Capacity + tilesWide - 1) / tilesWide) * _tileSource.TileHeight;

                return new System.Drawing.Rectangle(0, 0, width, height);
            }
        }

        protected override void AddSelectedTile (TileCoord coord)
        {
            int tilesWide = Width / _tileSource.TileWidth;

            _selectedTiles.Add(coord.Y * tilesWide + coord.X);
        }

        protected override void ClearSelectedTiles ()
        {
            _selectedTiles.Clear();
        }
    }
}
