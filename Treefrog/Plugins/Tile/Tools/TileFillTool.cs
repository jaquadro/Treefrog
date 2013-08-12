using System;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Plugins.Tiles.Commands;
using Treefrog.Presentation;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Tools;

namespace Treefrog.Plugins.Tiles.Tools
{
    public class TileFillTool : TilePointerTool
    {
        private TileReplace2DCommand _fillCommand;

        public TileFillTool (CommandHistory history, MultiTileGridLayer layer)
            : base(history, layer)
        { }

        public TileFillTool (CommandHistory history, MultiTileGridLayer layer, TileSourceType sourceType)
            : base(history, layer, sourceType)
        { }

        protected override void StartPointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    StartFillSequence(info);
                    break;
            }

            UpdatePointerSequence(info, viewport);
        }

        #region Fill Sequence

        private void StartFillSequence (PointerEventInfo info)
        {
            if (!SourceValid)
                return;

            TileCoord location = TileLocation(info);
            if (!TileInRange(location))
                return;

            if (SourceType == TileSourceType.Brush) {
                Layer.TileAdding += TileAddingHandler;
                Layer.TileRemoving += TileRemovingHandler;
            }

            _fillLayer = Layer;
            _sourceStack = new TileStack();
            _sourceStack.Add(ActiveTile);

            _fillCommand = new TileReplace2DCommand(Layer);
            FloodFill(location.X, location.Y);

            if (SourceType == TileSourceType.Brush) {
                Layer.TileAdding -= TileAddingHandler;
                Layer.TileRemoving -= TileRemovingHandler;
            }

            History.Execute(_fillCommand);
        }

        private void TileAddingHandler (object sender, LocatedTileEventArgs e)
        {
            _fillCommand.QueueReplacement(new TileCoord(e.X, e.Y), e.Tile);
        }

        private void TileRemovingHandler (object sender, LocatedTileEventArgs e)
        {
            _fillCommand.QueueRemove(new TileCoord(e.X, e.Y), e.Tile);
        }

        #endregion

        #region Fill Algorithm

        private MultiTileGridLayer _fillLayer;
        private TileStack _sourceStack;
        private TileStack _matchStack;

        private FloodFillRangeQueue _ranges;

        public void FloodFill (int x, int y)
        {
            _ranges = new FloodFillRangeQueue((_fillLayer.TilesWide + _fillLayer.TilesHigh) / 2 * 5);

            _matchStack = _fillLayer[x, y];

            LinearFill(ref x, ref y);

            while (_ranges.Count > 0) {
                FloodFillRange range = _ranges.Dequeue();

                int upY = range.Y - 1;
                int downY = range.Y + 1;

                TileCoord tid;
                for (int i = range.StartX; i <= range.EndX; i++) {
                    tid = new TileCoord(i, upY);
                    if (range.Y > 0 && /*!_sourceStack.Equals(_fillLayer[tid]) &&*/
                        (_matchStack == null ? _matchStack == _fillLayer[tid] : _matchStack.Equals(_fillLayer[tid])))
                        LinearFill(ref i, ref upY);

                    tid = new TileCoord(i, downY);
                    if (range.Y < (_fillLayer.TilesHigh - 1) && /*!_sourceStack.Equals(_fillLayer[tid]) &&*/
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

                if (SourceType == TileSourceType.Brush && ActiveBrush != null) {
                    ActiveBrush.ApplyBrush(_fillLayer, tid.X, tid.Y);
                }
                else {
                    _fillCommand.QueueReplacement(tid, _sourceStack);
                    _fillLayer[tid] = _sourceStack;
                }

                lFillLoc--;
                tid = new TileCoord(lFillLoc, y);

                if (lFillLoc < 0 || /*_sourceStack.Equals(_fillLayer[tid]) ||*/
                    !(_matchStack == null ? _matchStack == _fillLayer[tid] : _matchStack.Equals(_fillLayer[tid])))
                    break;
            }
            lFillLoc++;

            // Find right edge
            int rFillLoc = x;

            while (true) {
                TileCoord tid = new TileCoord(rFillLoc, y);

                /*if (!_sourceStack.Equals(_fillLayer[tid])) {*/
                    if (SourceType == TileSourceType.Brush && ActiveBrush != null) {
                        ActiveBrush.ApplyBrush(_fillLayer, tid.X, tid.Y);
                    }
                    else {
                        _fillCommand.QueueReplacement(tid, _sourceStack);
                        _fillLayer[tid] = _sourceStack;
                    }
                //}

                rFillLoc++;
                tid = new TileCoord(rFillLoc, y);

                if (rFillLoc >= _fillLayer.TilesWide || /*_sourceStack.Equals(_fillLayer[tid]) ||*/
                    !(_matchStack == null ? _matchStack == _fillLayer[tid] : _matchStack.Equals(_fillLayer[tid])))
                    break;
            }
            rFillLoc--;

            FloodFillRange r = new FloodFillRange(lFillLoc, rFillLoc, y);
            _ranges.Enqueue(ref r);
        }

        #endregion

        #region Fill Support Classes

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

        #endregion
    }
}
