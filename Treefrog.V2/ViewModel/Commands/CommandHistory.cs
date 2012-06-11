using System;
using System.Collections.Generic;

namespace Treefrog.ViewModel.Commands
{
    public class CommandHistory
    {
        private LinkedList<Command> _undoStack;
        private LinkedList<Command> _redoStack;

        private int _limit;

        public event EventHandler<CommandHistoryEventArgs> HistoryChanged;

        public CommandHistory ()
            : this(100)
        {
        }

        public CommandHistory (int limit)
        {
            _undoStack = new LinkedList<Command>();
            _redoStack = new LinkedList<Command>();
            _limit = limit;
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

            _undoStack.AddLast(command);
            _redoStack.Clear();

            if (_undoStack.Count > _limit)
                _undoStack.RemoveFirst();

            OnHistoryChanged(new CommandHistoryEventArgs(this));
        }

        public void Undo ()
        {
            if (_undoStack.Count == 0) {
                return;
            }

            Command command = _undoStack.Last.Value;
            _undoStack.RemoveLast();

            command.Undo();

            _redoStack.AddLast(command);

            OnHistoryChanged(new CommandHistoryEventArgs(this));
        }

        public void Redo ()
        {
            if (_redoStack.Count == 0) {
                return;
            }

            Command command = _redoStack.Last.Value;
            _redoStack.RemoveLast();

            command.Redo();

            _undoStack.AddLast(command);

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
