using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvalonDock.Controls
{
    interface ILogicalChildrenContainer
    {
        void InternalAddLogicalChild(object element);

        void InternalRemoveLogicalChild(object element);
    }
}
