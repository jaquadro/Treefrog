using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Framework;

namespace Treefrog.ViewModel.Annotations
{
    public abstract class Annotation
    {
    }

    public class SelectionAnnot : Annotation
    {
        public SelectionAnnot ()
            : this(new Point(0, 0))
        {
        }

        public SelectionAnnot (Point start)
        {
            Start = start;
            End = start;
        }

        public Point Start { get; set; }
        public Point End { get; set; }

        public Brush Fill { get; set; }
        public Pen Outline { get; set; }

        public void Normalize ()
        {
            Point newStart = new Point(Math.Min(Start.X, End.X), Math.Min(Start.Y, End.Y));
            Point newEnd = new Point(Math.Max(Start.X, End.X), Math.Max(Start.Y, End.Y));

            Start = newStart;
            End = newEnd;
        }

        public void MoveTo (Point location)
        {
            int diffX = location.X - Start.X;
            int diffY = location.Y - Start.Y;
            MoveBy(diffX, diffY);
        }

        public void MoveBy (int diffX, int diffY)
        {
            Start = new Point(Start.X + diffX, Start.Y + diffY);
            End = new Point(End.X + diffX, End.Y + diffY);
        }
    }

    public class MultiTileSelectionAnnot : Annotation
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

        internal HashSet<TileCoord> TileLocations
        {
            get { return _selectedLocations; }
        }

        internal TileCoord TileMinExtant { get; private set; }
        internal TileCoord TileMaxExtant { get; private set; }

        public int TileWidth { get; set; }
        public int TileHeight { get; set; }

        private Brush _fill;

        public Brush Fill
        {
            get { return _fill; }
            set
            {
                if (_fill != value) {
                    _fill = value;
                    OnFillInvalidated(EventArgs.Empty);
                }
            }
        }

        private Pen _outline;

        public Pen Outline
        {
            get { return _outline; }
            set
            {
                if (_outline != value) {
                    _outline = value;
                    OnOutlineInvalidated(EventArgs.Empty);
                }
            }
        }

        public TileCoord Offset { get; set; }

        public event EventHandler FillInvalidated;
        public event EventHandler OutlineInvalidated;

        protected virtual void OnFillInvalidated (EventArgs e)
        {
            var ev = FillInvalidated;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnOutlineInvalidated (EventArgs e)
        {
            var ev = OutlineInvalidated;
            if (ev != null)
                ev(this, e);
        }

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

        public void RemoveTileLocation (TileCoord location)
        {
            _selectedLocations.Remove(location);
            ShrinkExtants(location);
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
