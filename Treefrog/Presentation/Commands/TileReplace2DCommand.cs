using System;
using System.Collections.Generic;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Tools;
using Treefrog.Framework.Model.Support;

namespace Treefrog.Presentation.Commands
{
    public class CreateFloatingSelectionCommand : Command
    {
        private MultiTileGridLayer _tileSource;
        private FloatingTileSelectionOld _selection;

        public CreateFloatingSelectionCommand (MultiTileGridLayer source, FloatingTileSelectionOld selection)
        {
            _tileSource = source;
            _selection = selection;
        }

        public override void Execute ()
        {
            foreach (LocatedTileStack kv in _selection) {
                _tileSource[kv.Location] = null;
            }
        }

        public override void Undo ()
        {
            foreach (LocatedTileStack kv in _selection) {
                _tileSource[kv.Location] = kv.Stack != null ? new TileStack(kv.Stack) : null;
            }
        }

        public override void Redo ()
        {
            foreach (LocatedTileStack kv in _selection) {
                _tileSource[kv.Location] = null;
            }
        }
    }

    public class TileReplace2DCommand : Command
    {
        private struct TileRecord
        {
            public TileStack Original;
            public TileStack Replacement;

            public TileRecord (TileStack original, TileStack replacement)
            {
                Original = original;
                Replacement = replacement;
            }
        }

        private MultiTileGridLayer _tileSource;
        private Dictionary<TileCoord, TileRecord> _tiles;

        public TileReplace2DCommand (MultiTileGridLayer source)
        {
            _tileSource = source;
            _tiles = new Dictionary<TileCoord, TileRecord>();
        }

        public TileReplace2DCommand (MultiTileGridLayer source, Dictionary<TileCoord, Tile> tileData)
            : this(source)
        {
            foreach (KeyValuePair<TileCoord, Tile> kv in tileData) {
                TileStack stack = new TileStack();
                stack.Add(kv.Value);

                _tiles[kv.Key] = new TileRecord(_tileSource[kv.Key], stack);
            }
        }

        public TileReplace2DCommand (MultiTileGridLayer source, Dictionary<TileCoord, TileStack> tileData)
            : this(source)
        {
            foreach (KeyValuePair<TileCoord, TileStack> kv in tileData) {
                _tiles[kv.Key] = new TileRecord(_tileSource[kv.Key], kv.Value);
            }
        }

        public void QueueAdd (TileCoord coord, Tile tile)
        {
            if (tile != null) {
                TileStack stack = new TileStack(_tileSource[coord]);
                stack.Add(tile);

                if (_tiles.ContainsKey(coord))
                    _tiles[coord] = new TileRecord(_tiles[coord].Original, stack);
                else
                    _tiles[coord] = new TileRecord(new TileStack(_tileSource[coord]), stack);
            }
        }

        public void QueueAdd (TileCoord coord, TileStack stack)
        {
            if (stack != null) {
                TileStack newstack = new TileStack(_tileSource[coord]);
                foreach (Tile t in stack) {
                    newstack.Add(t);
                }

                if (_tiles.ContainsKey(coord))
                    _tiles[coord] = new TileRecord(_tiles[coord].Original, stack);
                else
                    _tiles[coord] = new TileRecord(new TileStack(_tileSource[coord]), newstack);
            }
        }

        public void QueueRemove (TileCoord coord, Tile tile)
        {
            if (tile != null) {
                TileStack stack = new TileStack(_tileSource[coord]);
                stack.Remove(tile);

                if (_tiles.ContainsKey(coord))
                    _tiles[coord] = new TileRecord(_tiles[coord].Original, stack);
                else
                    _tiles[coord] = new TileRecord(new TileStack(_tileSource[coord]), stack);
            }
        }

        public void QueueReplacement (TileCoord coord, Tile replacement)
        {
            TileStack stack = null;
            if (replacement != null) {
                stack = new TileStack();
                stack.Add(replacement);
            }

            _tiles[coord] = new TileRecord(new TileStack(_tileSource[coord]), stack);
        }

        public void QueueReplacement (TileCoord coord, TileStack replacement)
        {
            replacement = (replacement != null) ? new TileStack(replacement) : null;
            _tiles[coord] = new TileRecord(new TileStack(_tileSource[coord]), replacement);
        }

        public override void Execute ()
        {
            foreach (KeyValuePair<TileCoord, TileRecord> kv in _tiles) {
                _tileSource[kv.Key] = (kv.Value.Replacement != null) ? new TileStack(kv.Value.Replacement) : null;
            }
        }

        public override void Undo ()
        {
            foreach (KeyValuePair<TileCoord, TileRecord> kv in _tiles) {
                _tileSource[kv.Key] = (kv.Value.Original != null) ? new TileStack(kv.Value.Original) : null;
            }
        }

        public override void Redo ()
        {
            foreach (KeyValuePair<TileCoord, TileRecord> kv in _tiles) {
                _tileSource[kv.Key] = (kv.Value.Replacement != null) ? new TileStack(kv.Value.Replacement) : null;
            }
        }
    }
}
