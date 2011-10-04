using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Editor.Model;
using Microsoft.Xna.Framework;
using System.Windows.Forms;

namespace Editor
{
    public class TileControl2D : TileControl
    {
        private ITileSource2D _tileSource;

        private HashSet<TileCoord> _selectedTiles;

        public TileControl2D ()
            : base()
        {
            _selectedTiles = new HashSet<TileCoord>();
        }

        public override ITileSource TileSource
        {
            get { return base.TileSource; }
            set
            {
                if (value != null && value as ITileSource2D == null) {
                    throw new ArgumentException("Expected an ITileSource2D");
                }

                _tileSource = value as ITileSource2D;
                base.TileSource = value;
            }
        }

        public override System.Drawing.Rectangle VirtualSize
        {
            get
            {
                if (_tileSource == null) {
                    return new System.Drawing.Rectangle(0, 0, 0, 0);
                }

                return new System.Drawing.Rectangle(0, 0, _tileSource.PixelsWide, _tileSource.PixelsHigh);
            }
        }

        protected override IEnumerable<TileCoord> SelectedTileLocations
        {
            get
            {
                foreach (TileCoord tc in _selectedTiles) {
                    yield return tc;
                }
            }
        }

        protected override IEnumerable<KeyValuePair<TileCoord, Tile>> SelectedTiles
        {
            get
            {
                foreach (TileCoord tc in _selectedTiles) {
                    yield return new KeyValuePair<TileCoord, Tile>(tc, _tileSource[tc]);
                }
            }
        }

        protected override void DrawTiles (Vector2 offset, Rectangle region)
        {
            foreach (KeyValuePair<TileCoord, Tile> kvp in _tileSource.Region(region)) {
                Rectangle dest = new Rectangle(
                    kvp.Key.X * (int)(_tileSource.TileWidth * Zoom),
                    kvp.Key.Y * (int)(_tileSource.TileHeight * Zoom),
                    (int)(_tileSource.TileWidth * Zoom),
                    (int)(_tileSource.TileHeight * Zoom));
                kvp.Value.Draw(_spriteBatch, dest);
            }
        }

        protected override void AddSelectedTile (TileCoord coord)
        {
            _selectedTiles.Add(coord);
        }

        protected override void ClearSelectedTiles ()
        {
            _selectedTiles.Clear();
        }

        private bool _isDragging;

        protected override void OnMouseTileDown (TileMouseEventArgs e)
        {
            if (!IsSelecting && _selectedTiles.Contains(e.TileLocation)) {
                _isDragging = true;
                HoldSelection = true;
            }

            base.OnMouseTileDown(e);
        }

        protected override void OnMouseTileMove (TileMouseEventArgs e)
        {
            base.OnMouseTileMove(e);

            if (_isDragging || (!IsSelecting && _selectedTiles.Contains(e.TileLocation))) {
                Cursor = Cursors.SizeAll;
            }
            else {
                Cursor = Cursors.Default;
            }
        }

        protected override void OnMouseTileUp (TileMouseEventArgs e)
        {
            base.OnMouseTileUp(e);

            if (_isDragging) {
                _isDragging = false;
                HoldSelection = false;
            }
        }
    }
}
