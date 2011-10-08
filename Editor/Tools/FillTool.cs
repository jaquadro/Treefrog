using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Editor.Model;
using Microsoft.Xna.Framework;
using Editor.Model.Controls;

namespace Editor
{
    public class FillTool : MouseTool
    {
        private TileControlLayer _source;
        private TileSet2D _selectedTileSet;
        private CommandHistory _commandHistory;

        private Tile _sourceTile;
        private TileReplace2DCommand _fillCommand;

        public FillTool (TileControl2D control, TileControlLayer source, TileSet2D tileset, CommandHistory commandHistory)
            : base(control)
        {
            _selectedTileSet = tileset;
            _commandHistory = commandHistory;
            _source = source;
        }

        protected override void AttachHandlers ()
        {
            base.AttachHandlers();
            //_source.TileSelected += SourceTileSelected;
        }

        protected override void DetachHandlers ()
        {
            base.DetachHandlers();
            //_source.TileSelected -= SourceTileSelected;
        }

        protected virtual void SourceTileSelectedHandler (object sender, TileEventArgs e)
        {
            _sourceTile = e.Tile;
        }

        protected virtual void SourceTileMultiSelectedHandler (object sender, TileSelectEventArgs e)
        {

        }

        protected override void MouseTileClickHandler (object sender, TileMouseEventArgs e)
        {
            if (e.TileLocation.X < 0 || e.TileLocation.X >= _selectedTileSet.TilesWide)
                return;
            if (e.TileLocation.Y < 0 || e.TileLocation.Y >= _selectedTileSet.TilesHigh)
                return;

            _fillCommand = new TileReplace2DCommand(_selectedTileSet);
            FloodFill(new Point(e.TileLocation.X, e.TileLocation.Y));
            _commandHistory.Execute(_fillCommand);
        }

        #region Private

        private void SourceTileSelected (object sender, TileEventArgs e)
        {
            SourceTileSelectedHandler(sender, e);
        }

        private void SourceTileMultiSelected (object sender, TileSelectEventArgs e)
        {
            SourceTileMultiSelectedHandler(sender, e);
        }

        #endregion

        #region Fill Algorithm

        private Tile _matchTile;

        private FloodFillRangeQueue _ranges;

        public void FloodFill (Point point)
        {
            _ranges = new FloodFillRangeQueue((_selectedTileSet.TilesWide + _selectedTileSet.TilesHigh) / 2 * 5);

            int x = point.X;
            int y = point.Y;

            _matchTile = _selectedTileSet[new TileCoord(x, y)];

            LinearFill(ref x, ref y);

            while (_ranges.Count > 0) {
                FloodFillRange range = _ranges.Dequeue();

                int upY = range.Y - 1;
                int downY = range.Y + 1;

                TileCoord tid;
                for (int i = range.StartX; i <= range.EndX; i++) {
                    tid = new TileCoord(i, upY);
                    if (range.Y > 0 && _selectedTileSet[tid] != _sourceTile && _selectedTileSet[tid] == _matchTile)
                        LinearFill(ref i, ref upY);

                    tid = new TileCoord(i, downY);
                    if (range.Y < (_selectedTileSet.TilesHigh - 1) && _selectedTileSet[tid] != _sourceTile && _selectedTileSet[tid] == _matchTile)
                        LinearFill(ref i, ref downY);
                }
            }
        }

        private void LinearFill (ref int x, ref int y)
        {
            // Find left edge
            int lFillLoc = x;

            while (true) {
                TileCoord tid = new TileCoord(lFillLoc, y);

                _fillCommand.QueueReplacement(tid, _sourceTile);
                _selectedTileSet[tid] = _sourceTile;

                lFillLoc--;
                tid = new TileCoord(lFillLoc, y);

                if (lFillLoc < 0 || _selectedTileSet[tid] == _sourceTile || _selectedTileSet[tid] != _matchTile)
                    break;
            }
            lFillLoc++;

            // Find right edge
            int rFillLoc = x;

            while (true) {
                TileCoord tid = new TileCoord(rFillLoc, y);

                if (_selectedTileSet[tid] != _sourceTile) {
                    _fillCommand.QueueReplacement(tid, _sourceTile);
                    _selectedTileSet[tid] = _sourceTile;
                }

                rFillLoc++;
                tid = new TileCoord(rFillLoc, y);

                if (rFillLoc >= _selectedTileSet.TilesWide || _selectedTileSet[tid] == _sourceTile || _selectedTileSet[tid] != _matchTile)
                    break;
            }
            rFillLoc--;

            FloodFillRange r = new FloodFillRange(lFillLoc, rFillLoc, y);
            _ranges.Enqueue(ref r);
        }

        #endregion
    }

    public class FloodFillRangeQueue
    {
        FloodFillRange[] array;
        int size;
        int head;

        public int Count
        {
            get { return size; }
        }

        public FloodFillRangeQueue (int initialSize)
        {
            array = new FloodFillRange[initialSize];
            head = 0;
            size = 0;
        }

        public FloodFillRange First
        {
            get { return array[head]; }
        }

        public void Enqueue (ref FloodFillRange r)
        {
            if (size + head == array.Length) {
                FloodFillRange[] newArray = new FloodFillRange[2 * array.Length];
                Array.Copy(array, head, newArray, 0, size);
                array = newArray;
                head = 0;
            }
            array[head + (size++)] = r;
        }

        public FloodFillRange Dequeue ()
        {
            FloodFillRange range = new FloodFillRange();
            if (size > 0) {
                range = array[head];
                array[head] = new FloodFillRange();
                head++;
                size--;
            }
            return range;
        }
    }

    public struct FloodFillRange
    {
        public int StartX;
        public int EndX;
        public int Y;

        public FloodFillRange (int startX, int endX, int y)
        {
            StartX = startX;
            EndX = endX;
            Y = y;
        }
    }
}
