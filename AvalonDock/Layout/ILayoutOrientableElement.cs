using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace AvalonDock.Layout
{
    public interface ILayoutOrientableGroup : ILayoutGroup
    {
        Orientation Orientation { get; set; }
    }
}
