using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Treefrog.Presentation
{
    public interface IViewport
    {
        Point Offset { get; }
        Size Viewport { get; }
        Size Limit { get; }
        float ZoomFactor { get; }

        Rectangle VisibleRegion { get; }
    }
}
