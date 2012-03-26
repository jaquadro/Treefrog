using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace AvalonDock.Controls
{
    internal interface IOverlayWindowArea
    {
        Rect ScreenDetectionArea { get; }
    }
}
