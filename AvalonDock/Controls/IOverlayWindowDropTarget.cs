using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace AvalonDock.Controls
{
    interface IOverlayWindowDropTarget
    {
        Rect ScreenDetectionArea { get; }

        OverlayWindowDropTargetType Type { get; }
    }
}
