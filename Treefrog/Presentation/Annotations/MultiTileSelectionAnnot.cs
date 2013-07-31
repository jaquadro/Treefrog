using System;
using System.Collections.Generic;
using Treefrog.Framework;
using Treefrog.Framework.Imaging;

namespace Treefrog.Presentation.Annotations
{
    public class MultiTileSelectionAnnot : DrawAnnotation
    {
        private HashSet<TileCoord> _selectedLocations;

        public MultiTileSelectionAnnot ()
            : this(new Point(0, 0))
        {
        }

        public MultiTileSelectionAnnot (Point start)
        {
            _selectedLocations = new HashSet<TileCoord>();

            TileMinExtant = new TileCoord(Int32.MaxValue, Int32.MaxValue);
            TileMaxExtant = new TileCoord(Int32.MinValue, Int32.MinValue);
        }

        public MultiTileSelectionAnnot (MultiTileSelectionAnnot annot)
        {
            if (annot != null) {
                TileMinExtant = annot.TileMinExtant;
                TileMaxExtant = annot.TileMaxExtant;
                TileWidth = annot.TileWidth;
                TileHeight = annot.TileHeight;

                Fill = annot.Fill;
                Outline = annot.Outline;

                Offset = annot.Offset;

                _selectedLocations = new HashSet<TileCoord>();
                foreach (TileCoord coord in annot._selectedLocations)
                    _selectedLocations.Add(coord);
            }
        }

        internal HashSet<TileCoord> TileLocations
        {
            get { return _selectedLocations; }
        }

        internal TileCoord TileMinExtant { get; private set; }
        internal TileCoord TileMaxExtant { get; private set; }

        public int TileWidth { get; set; }
        public int TileHeight { get; set; }

        public TileCoord Offset { get; set; }

        public void MoveTo (TileCoord location)
        {
            Offset = new TileCoord(location.X - TileMinExtant.X, location.Y - TileMinExtant.Y);
        }

        public void MoveBy (int diffX, int diffY)
        {
            Offset = new TileCoord(Offset.X + diffX, Offset.Y + diffY);
        }

        public void AddTileLocation (TileCoord location)
        {
            _selectedLocations.Add(location);
            ExpandExtants(location);
        }

        public void AddTileLocation (Rectangle region)
        {
            for (int y = region.Top; y < region.Bottom; y++)
                for (int x = region.Left; x < region.Right; x++)
                    AddTileLocation(new TileCoord(x, y));
        }

        public void RemoveTileLocation (TileCoord location)
        {
            _selectedLocations.Remove(location);
            ShrinkExtants(location);
        }

        public void RemoveTileLocation (Rectangle region)
        {
            for (int y = region.Top; y < region.Bottom; y++)
                for (int x = region.Left; x < region.Right; x++)
                    RemoveTileLocation(new TileCoord(x, y));
        }

        private void ExpandExtants (TileCoord location)
        {
            if (location.X < TileMinExtant.X)
                TileMinExtant = new TileCoord(location.X, TileMinExtant.Y);
            if (location.Y < TileMinExtant.Y)
                TileMinExtant = new TileCoord(TileMinExtant.X, location.Y);

            if (location.X > TileMaxExtant.X)
                TileMaxExtant = new TileCoord(location.X, TileMaxExtant.Y);
            if (location.Y > TileMaxExtant.Y)
                TileMaxExtant = new TileCoord(TileMaxExtant.X, location.Y);
        }

        private void ShrinkExtants (TileCoord location)
        {
            if (location.X == TileMinExtant.X
                || location.X == TileMaxExtant.X
                || location.Y == TileMinExtant.Y
                || location.Y == TileMaxExtant.Y)
                RecalculateExtants();
        }

        private void RecalculateExtants ()
        {
            TileMinExtant = new TileCoord(Int32.MaxValue, Int32.MaxValue);
            TileMaxExtant = new TileCoord(Int32.MinValue, Int32.MinValue);

            foreach (TileCoord coord in _selectedLocations)
                ExpandExtants(coord);
        }
    }
}
