using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Editor
{
    public class CommandHistoryEventArgs : EventArgs
    {
        private CommandHistory _commandHistory;

        public CommandHistoryEventArgs (CommandHistory commandHistory)
        {
            _commandHistory = commandHistory;
        }

        public CommandHistory CommandHistory
        {
            get { return _commandHistory; }
        }
    }

    public class ClipboardEventArgs : EventArgs
    {
        private bool _selectionReady;
        private bool _dataReady;

        public ClipboardEventArgs (bool selectionReady, bool dataReady)
        {
            _selectionReady = selectionReady;
            _dataReady = dataReady;
        }

        public bool SelectionReady
        {
            get { return _selectionReady; }
        }

        public bool DataReady
        {
            get { return _dataReady; }
        }
    }
}
