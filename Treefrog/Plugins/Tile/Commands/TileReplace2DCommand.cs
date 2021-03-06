﻿using System.Collections.Generic;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;

namespace Treefrog.Plugins.Tiles.Commands
{
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

        public bool IsEmpty
        {
            get { return _tiles.Count == 0; }
        }

        public void QueueAdd (TileCoord coord, Tile tile)
        {
            if (tile != null) {
                TileStack srcStack = null;
                if (_tileSource.InRange(coord))
                    srcStack = new TileStack(_tileSource[coord]);

                TileStack stack = new TileStack(srcStack);
                stack.Add(tile);

                if (_tiles.ContainsKey(coord))
                    _tiles[coord] = new TileRecord(_tiles[coord].Original, stack);
                else
                    _tiles[coord] = new TileRecord(srcStack, stack);
            }
        }

        public void QueueAdd (TileCoord coord, TileStack stack)
        {
            if (stack != null) {
                TileStack srcStack = null;
                if (_tileSource.InRange(coord))
                    srcStack = new TileStack(_tileSource[coord]);

                TileStack newStack = new TileStack(srcStack);
                foreach (Tile t in stack)
                    newStack.Add(t);

                if (_tiles.ContainsKey(coord))
                    _tiles[coord] = new TileRecord(_tiles[coord].Original, stack);
                else
                    _tiles[coord] = new TileRecord(srcStack, newStack);
            }
        }

        public void QueueRemove (TileCoord coord, Tile tile)
        {
            if (tile != null) {
                TileStack srcStack = null;
                if (_tileSource.InRange(coord))
                    srcStack = new TileStack(_tileSource[coord]);

                TileStack stack = new TileStack(srcStack);
                stack.Remove(tile);

                if (_tiles.ContainsKey(coord))
                    _tiles[coord] = new TileRecord(_tiles[coord].Original, stack);
                else
                    _tiles[coord] = new TileRecord(srcStack, stack);
            }
        }

        public void QueueReplacement (TileCoord coord, Tile replacement)
        {
            TileStack srcStack = null;
            if (_tileSource.InRange(coord))
                srcStack = new TileStack(_tileSource[coord]);

            TileStack stack = null;
            if (replacement != null) {
                stack = new TileStack();
                stack.Add(replacement);
            }

            if (_tiles.ContainsKey(coord))
                _tiles[coord] = new TileRecord(_tiles[coord].Original, stack);
            else
                _tiles[coord] = new TileRecord(srcStack, stack);
        }

        public void QueueReplacement (TileCoord coord, TileStack replacement)
        {
            TileStack srcStack = null;
            if (_tileSource.InRange(coord))
                srcStack = new TileStack(_tileSource[coord]);

            replacement = (replacement != null) ? new TileStack(replacement) : null;

            if (_tiles.ContainsKey(coord))
                _tiles[coord] = new TileRecord(_tiles[coord].Original, replacement);
            else
                _tiles[coord] = new TileRecord(srcStack, replacement);
        }

        public override void Execute ()
        {
            foreach (KeyValuePair<TileCoord, TileRecord> kv in _tiles) {
                if (_tileSource.InRange(kv.Key))
                    _tileSource[kv.Key] = (kv.Value.Replacement != null && kv.Value.Replacement.Count > 0) ? new TileStack(kv.Value.Replacement) : null;
            }
        }

        public override void Undo ()
        {
            foreach (KeyValuePair<TileCoord, TileRecord> kv in _tiles) {
                if (_tileSource.InRange(kv.Key))
                    _tileSource[kv.Key] = (kv.Value.Original != null && kv.Value.Original.Count > 0) ? new TileStack(kv.Value.Original) : null;
            }
        }

        public override void Redo ()
        {
            foreach (KeyValuePair<TileCoord, TileRecord> kv in _tiles) {
                if (_tileSource.InRange(kv.Key))
                    _tileSource[kv.Key] = (kv.Value.Replacement != null && kv.Value.Replacement.Count > 0) ? new TileStack(kv.Value.Replacement) : null;
            }
        }
    }
}
