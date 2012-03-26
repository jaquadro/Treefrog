using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace AvalonDock.Controls
{
    public class DockingManagerOverlayArea : OverlayArea
    {
        internal DockingManagerOverlayArea(IOverlayWindow overlayWindow, DockingManager manager)
            : base(overlayWindow)
        {
            _manager = manager;

            base.SetScreenDetectionArea(new Rect(
                _manager.PointToScreenDPI(new Point()),
                _manager.TransformActualSizeToAncestor()));
        }

        DockingManager _manager;

    }
}
