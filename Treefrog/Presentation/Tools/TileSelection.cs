using System.Collections.Generic;
using Treefrog.Framework;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Framework.Model;
using Treefrog.Framework.Model.Support;
using Treefrog.Presentation.Annotations;

namespace Treefrog.Prseentation.Tools
{
    public class TileSelection
    {
        private static Brush InactiveFill = new SolidColorBrush(new Color(192, 192, 192, 96));
        private static Pen InactiveOutline = null;

        private static Brush FloatingFill = new SolidColorBrush(new Color(128, 76, 255, 128));
        private static Pen FloatingOutline = null;

        private static Brush SelectedFill = new SolidColorBrush(new Color(76, 178, 255, 128));
        private static Pen SelectedOutline = null;

        private Dictionary<TileCoord, TileStack> _tiles;
        private MultiTileSelectionAnnot _tileAnnot;

        private TileCoord _offset;

        private TileSelection ()
        {
            _tiles = new Dictionary<TileCoord, TileStack>();
            _offset = new TileCoord(0, 0);
            _tileAnnot = new MultiTileSelectionAnnot();
        }

        public TileSelection (int tileWidth, int tileHeight)
            : this()
        {
            _tileAnnot.TileWidth = tileWidth;
            _tileAnnot.TileHeight = tileHeight;

            Activate();
        }

        public TileSelection (TileSelection selection)
        {
            if (selection != null) {
                _offset = selection._offset;

                _tileAnnot = new MultiTileSelectionAnnot(selection._tileAnnot);

                _active = selection._active;
                _floating = selection._floating;

                _tiles = new Dictionary<TileCoord, TileStack>();
                foreach (KeyValuePair<TileCoord, TileStack> kvp in selection._tiles)
                    _tiles.Add(kvp.Key, new TileStack(kvp.Value));
            }
        }

        #region Floating State

        private bool _floating;

        public bool Floating
        {
            get { return _floating; }
        }

        public void Float ()
        {
            _floating = true;

            if (_active) {
                _tileAnnot.Fill = FloatingFill;
                _tileAnnot.Outline = FloatingOutline;
            }
        }

        public void Defloat ()
        {
            _floating = false;

            if (_active) {
                _tileAnnot.Fill = SelectedFill;
                _tileAnnot.Outline = SelectedOutline;
            }
        }

        #endregion

        #region Active State

        private bool _active;

        public bool Active
        {
            get { return _active; }
        }

        public void Activate ()
        {
            _active = true;

            if (_floating)
                Float();
            else
                Defloat();
        }

        public void Deactivate ()
        {
            _active = false;
            _tileAnnot.Fill = InactiveFill;
            _tileAnnot.Outline = InactiveOutline;
        }

        #endregion

        #region Tile Collection

        public IDictionary<TileCoord, TileStack> Tiles
        {
            get { return _tiles; }
        }

        private void AddTile (MultiTileGridLayer layer, TileCoord location)
        {
            if (!_tiles.ContainsKey(location)) {
                TileStack stack = layer.TileStacksAt(location);
                if (!TileStack.NullOrEmpty(stack))
                    _tiles.Add(location, stack);
                _tileAnnot.AddTileLocation(location);
            }
        }

        public void AddTiles (MultiTileGridLayer layer, IEnumerable<TileCoord> tileLocations)
        {
            foreach (TileCoord coord in tileLocations) {
                AddTile(layer, coord);
            }
        }

        public void AddTiles (MultiTileGridLayer layer, Rectangle tileRegion)
        {
            for (int y = tileRegion.Top; y < tileRegion.Bottom; y++) {
                for (int x = tileRegion.Left; x < tileRegion.Right; x++) {
                    AddTile(layer, new TileCoord(x, y));
                }
            }
        }

        public void AddTiles (IDictionary<TileCoord, TileStack> tiles)
        {
            foreach (KeyValuePair<TileCoord, TileStack> kvp in tiles) {
                if (!_tiles.ContainsKey(kvp.Key)) {
                    if (!TileStack.NullOrEmpty(kvp.Value))
                        _tiles.Add(kvp.Key, kvp.Value);
                    _tileAnnot.AddTileLocation(kvp.Key);
                }
            }
        }

        public void AddTiles (IEnumerable<LocatedTileStack> tiles)
        {
            foreach (LocatedTileStack stack in tiles) {
                if (!_tiles.ContainsKey(stack.Location)) {
                    if (!TileStack.NullOrEmpty(stack.Stack))
                        _tiles.Add(stack.Location, stack.Stack);
                    _tileAnnot.AddTileLocation(stack.Location);
                }
            }
        }

        public void RemoveTiles (IEnumerable<TileCoord> tileLocations)
        {
            foreach (TileCoord coord in tileLocations) {
                _tiles.Remove(coord);
                _tileAnnot.RemoveTileLocation(coord);
            }
        }

        public void RemoveTiles (Rectangle tileRegion)
        {
            for (int y = tileRegion.Top; y < tileRegion.Bottom; y++) {
                for (int x = tileRegion.Left; x < tileRegion.Right; x++) {
                    _tiles.Remove(new TileCoord(x, y));
                    _tileAnnot.RemoveTileLocation(new TileCoord(x, y));
                }
            }
        }

        public void ClearTiles ()
        {
            _tiles.Clear();
            _offset = new TileCoord(0, 0);
        }

        #endregion

        private TileCoord AdjustLocation (TileCoord location)
        {
            return new TileCoord(location.X - _offset.X, location.Y - _offset.Y);
        }

        public bool TileAt (TileCoord location)
        {
            return _tiles.ContainsKey(AdjustLocation(location));
        }

        public bool CoverageAt (TileCoord location)
        {
            return _tileAnnot.TileLocations.Contains(AdjustLocation(location));
        }

        public Annotation SelectionAnnotation
        {
            get { return _tileAnnot; }
        }

        public TileCoord Offset
        {
            get { return _offset; }
            set
            {
                _offset = value;
                _tileAnnot.Offset = value;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(
                    _tileAnnot.TileMinExtant.X,
                    _tileAnnot.TileMinExtant.Y,
                    _tileAnnot.TileMaxExtant.X - _tileAnnot.TileMinExtant.X,
                    _tileAnnot.TileMaxExtant.Y - _tileAnnot.TileMinExtant.Y
                    );
            }
        }
    }
}
