using System;
using System.Collections.Generic;
using Treefrog.Framework;
using Treefrog.Framework.Model;

namespace Treefrog.Presentation.Commands
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

        public void QueueAdd (TileCoord coord, Tile tile)
        {
            TileStack stack = new TileStack(_tileSource[coord]);
            stack.Add(tile);

            _tiles[coord] = new TileRecord(new TileStack(_tileSource[coord]), stack);
        }

        public void QueueReplacement (TileCoord coord, Tile replacement)
        {
            TileStack stack = new TileStack();
            stack.Add(replacement);

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
                _tileSource[kv.Key] = new TileStack(kv.Value.Replacement);
            }
        }

        public override void Undo ()
        {
            foreach (KeyValuePair<TileCoord, TileRecord> kv in _tiles) {
                _tileSource[kv.Key] = new TileStack(kv.Value.Original);
            }
        }

        public override void Redo ()
        {
            foreach (KeyValuePair<TileCoord, TileRecord> kv in _tiles) {
                _tileSource[kv.Key] = new TileStack(kv.Value.Replacement);
            }
        }
    }
}
