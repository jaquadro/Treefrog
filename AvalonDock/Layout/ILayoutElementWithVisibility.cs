using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvalonDock.Layout
{
    public interface ILayoutElementWithVisibility
    {
        void ComputeVisibility();
    }
}
