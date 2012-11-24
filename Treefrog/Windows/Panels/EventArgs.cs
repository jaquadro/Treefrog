using System;

namespace Treefrog.Windows
{
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
