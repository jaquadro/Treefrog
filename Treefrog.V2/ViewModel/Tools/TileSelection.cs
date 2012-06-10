using System.Collections.Generic;
using Treefrog.Framework;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Framework.Model;
using Treefrog.ViewModel.Annotations;
using Treefrog.Framework.Model.Support;

namespace Treefrog.ViewModel.Tools
{
    public class TileSelection
    {
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
                _tileAnnot.Fill = new SolidColorBrush(new Color(128, 96, 216, 96));
                _tileAnnot.Outline = new Pen(new SolidColorBrush(new Color(80, 50, 180, 200)), 2);
            }
        }

        public void Defloat ()
        {
            _floating = false;

            if (_active) {
                _tileAnnot.Fill = new SolidColorBrush(new Color(64, 96, 216, 96));
                _tileAnnot.Outline = new Pen(new SolidColorBrush(new Color(40, 70, 190, 200)), 2);
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

            _tileAnnot.Fill = new SolidColorBrush(new Color(192, 192, 192, 96));
            _tileAnnot.Outline = new Pen(new SolidColorBrush(new Color(192, 192, 192, 200)), 2);
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
    }
}
