using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvalonDock.Layout;

namespace AvalonDock
{
    public class DocumentCloseEventArgs : EventArgs
    {
        public DocumentCloseEventArgs(LayoutDocument document)
        {
            Document = document;
        }

        public LayoutDocument Document
        {
            get;
            private set;
        }
    }
}
