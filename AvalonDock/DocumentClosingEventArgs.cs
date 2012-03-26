using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using AvalonDock.Layout;

namespace AvalonDock
{
    public class DocumentClosingEventArgs : CancelEventArgs
    {
        public DocumentClosingEventArgs(LayoutDocument document)
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
