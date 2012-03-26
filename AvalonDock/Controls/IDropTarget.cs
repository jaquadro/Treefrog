using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
    internal interface IDropTarget
    {
//        Rect DetectionRect { get; }
        Geometry GetPreviewPath(OverlayWindow overlayWindow, LayoutFloatingWindow floatingWindow);

        bool HitTest(Point dragPoint);

        DropTargetType Type { get; }

        void Drop(LayoutFloatingWindow floatingWindow);

    }
}
