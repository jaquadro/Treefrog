using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Editor.Model;

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
            public Tile Original;
            public Tile Replacement;

            public TileRecord (Tile original, Tile replacement)
            {
                Original = original;
                Replacement = replacement;
            }
        }

        private ITileSource2D _tileSource;
        private Dictionary<TileCoord, TileRecord> _tiles;

        public TileReplace2DCommand (ITileSource2D source)
        {
            _tileSource = source;
            _tiles = new Dictionary<TileCoord, TileRecord>();
        }

        public TileReplace2DCommand (ITileSource2D source, Dictionary<TileCoord, Tile> tileData)
            : this(source)
        {
            foreach (KeyValuePair<TileCoord, Tile> kv in tileData) {
                _tiles[kv.Key] = new TileRecord(_tileSource[kv.Key], kv.Value);
            }
        }

        public void QueueReplacement (TileCoord coord, Tile replacement)
        {
            _tiles[coord] = new TileRecord(_tileSource[coord], replacement);
        }

        public override void Execute ()
        {
            foreach (KeyValuePair<TileCoord, TileRecord> kv in _tiles) {
                _tileSource[kv.Key] = kv.Value.Replacement;
            }
        }

        public override void Undo ()
        {
            foreach (KeyValuePair<TileCoord, TileRecord> kv in _tiles) {
                _tileSource[kv.Key] = kv.Value.Original;
            }
        }

        public override void Redo ()
        {
            foreach (KeyValuePair<TileCoord, TileRecord> kv in _tiles) {
                _tileSource[kv.Key] = kv.Value.Replacement;
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
