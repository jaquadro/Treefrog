using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvalonDock.Layout.Serialization
{
    public class LayoutSerializationCallbackEventArgs : EventArgs
    {
        public LayoutSerializationCallbackEventArgs(LayoutContent model)
        {
            Model = model;
        }

        public LayoutContent Model { get; private set; }

        public object Content { get; set; }
    }
}
