using System;
using System.Collections.Generic;

namespace Treefrog.ViewModel.Commands
{
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
