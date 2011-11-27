using System;
using Microsoft.Xna.Framework;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Layers;

namespace Treefrog.Presentation.Tools
{
    public class FillTool
    {
        private LevelPresenter _level;
        private ITilePoolListPresenter _pool;
        private ILevelToolsPresenter _tools;

        private TileReplace2DCommand _fillCommand;

        public FillTool (LevelPresenter level)
        {
            _level = level;
            _level.SyncCurrentLayer += SyncLayerHandler;
        }

        public void BindTileSourceController (ITilePoolListPresenter controller)
        {
            _pool = controller;
        }

        public void BindLevelToolsController (ILevelToolsPresenter controller)
        {
            _tools = controller;
        }

        private void SyncLayerHandler (object sender, SyncLayerEventArgs e)
        {
            if (e.PreviousControlLayer != null) {
                TileControlLayer prev = e.PreviousControlLayer as TileControlLayer;
                if (prev != null) {
                    prev.MouseTileClick -= MouseTileClickHandler;
                }
            }

            TileControlLayer control = CurrentControlLayer;
            if (control != null) {
                control.MouseTileClick += MouseTileClickHandler;
            }
        }

        private bool ControllersAttached
        {
            get { return _level != null && _pool != null && _tools != null; }
        }

        private MultiTileGridLayer CurrentLayer
        {
            get
            {
                if (_level != null) {
                    return _level.SelectedLayer as MultiTileGridLayer;
                }

                return null;
            }
        }

        private TileControlLayer CurrentControlLayer
        {
            get
            {
                if (_level != null) {
                    return _level.SelectedControlLayer as TileControlLayer;
                }

                return null;
            }
        }

        protected void MouseTileClickHandler (object sender, TileMouseEventArgs e)
        {
            if (!ControllersAttached || _tools.ActiveTileTool != TileToolMode.Fill) {
                return;
            }

            MultiTileGridLayer layer = CurrentLayer;
            if (layer == null) {
                return;
            }

            if (_pool.SelectedTile == null) {
                return;
            }

            if (e.TileLocation.X < 0 || e.TileLocation.X >= layer.LayerWidth)
                return;
            if (e.TileLocation.Y < 0 || e.TileLocation.Y >= layer.LayerHeight)
                return;

            _fillLayer = layer;
            _sourceStack = new TileStack();
            _sourceStack.Add(_pool.SelectedTile);

            _fillCommand = new TileReplace2DCommand(layer);
            FloodFill(new Point(e.TileLocation.X, e.TileLocation.Y));
            _level.History.Execute(_fillCommand);
        }

        #region Fill Algorithm

        private MultiTileGridLayer _fillLayer;
        private TileStack _sourceStack;
        private TileStack _matchStack;

        private FloodFillRangeQueue _ranges;

        public void FloodFill (Point point)
        {
            _ranges = new FloodFillRangeQueue((_fillLayer.LayerWidth + _fillLayer.LayerHeight) / 2 * 5);

            int x = point.X;
            int y = point.Y;

            _matchStack = _fillLayer[x, y];

            LinearFill(ref x, ref y);

            while (_ranges.Count > 0) {
                FloodFillRange range = _ranges.Dequeue();

                int upY = range.Y - 1;
                int downY = range.Y + 1;

                TileCoord tid;
                for (int i = range.StartX; i <= range.EndX; i++) {
                    tid = new TileCoord(i, upY);
                    if (range.Y > 0 && !_sourceStack.Equals(_fillLayer[tid]) &&
                        (_matchStack == null ? _matchStack == _fillLayer[tid] : _matchStack.Equals(_fillLayer[tid])))
                        LinearFill(ref i, ref upY);

                    tid = new TileCoord(i, downY);
                    if (range.Y < (_fillLayer.LayerHeight - 1) && !_sourceStack.Equals(_fillLayer[tid]) && 
                        (_matchStack == null ? _matchStack == _fillLayer[tid] : _matchStack.Equals(_fillLayer[tid])))
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

                _fillCommand.QueueReplacement(tid, _sourceStack);
                _fillLayer[tid] = _sourceStack;

                lFillLoc--;
                tid = new TileCoord(lFillLoc, y);

                if (lFillLoc < 0 || _sourceStack.Equals(_fillLayer[tid]) || 
                    !(_matchStack == null ? _matchStack == _fillLayer[tid] : _matchStack.Equals(_fillLayer[tid])))
                    break;
            }
            lFillLoc++;

            // Find right edge
            int rFillLoc = x;

            while (true) {
                TileCoord tid = new TileCoord(rFillLoc, y);

                if (!_sourceStack.Equals(_fillLayer[tid])) {
                    _fillCommand.QueueReplacement(tid, _sourceStack);
                    _fillLayer[tid] = _sourceStack;
                }

                rFillLoc++;
                tid = new TileCoord(rFillLoc, y);

                if (rFillLoc >= _fillLayer.LayerWidth || _sourceStack.Equals(_fillLayer[tid]) ||
                    !(_matchStack == null ? _matchStack == _fillLayer[tid] : _matchStack.Equals(_fillLayer[tid])))
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
