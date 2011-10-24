using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework;
using Treefrog.Framework.Model;

namespace Editor
{
    public abstract class Command
    {
        public abstract void Execute ();

        public abstract void Undo ();

        public abstract void Redo ();
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
            _tiles[coord] = new TileRecord(new TileStack(_tileSource[coord]), new TileStack(replacement));
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

    public class CommandHistory
    {
        private Stack<Command> _undoStack;
        private Stack<Command> _redoStack;

        public event EventHandler<CommandHistoryEventArgs> HistoryChanged;

        public CommandHistory ()
        {
            _undoStack = new Stack<Command>();
            _redoStack = new Stack<Command>();
        }

        public bool CanRedo
        {
            get { return _redoStack.Count > 0; }
        }

        public bool CanUndo
        {
            get { return _undoStack.Count > 0; }
        }

        protected void OnHistoryChanged (CommandHistoryEventArgs e)
        {
            if (HistoryChanged != null) {
                HistoryChanged(this, e);
            }
        }

        public void Execute (Command command)
        {
            command.Execute();

            _undoStack.Push(command);
            _redoStack.Clear();

            OnHistoryChanged(new CommandHistoryEventArgs(this));
        }

        public void Undo ()
        {
            if (_undoStack.Count == 0) {
                return;
            }
            
            Command command = _undoStack.Pop();
            command.Undo();

            _redoStack.Push(command);

            OnHistoryChanged(new CommandHistoryEventArgs(this));
        }

        public void Redo ()
        {
            if (_redoStack.Count == 0) {
                return;
            }

            Command command = _redoStack.Pop();
            command.Redo();

            _undoStack.Push(command);

            OnHistoryChanged(new CommandHistoryEventArgs(this));
        }

        public void Clear ()
        {
            _undoStack.Clear();
            _redoStack.Clear();

            OnHistoryChanged(new CommandHistoryEventArgs(this));
        }
    }
}
