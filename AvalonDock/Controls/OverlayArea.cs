using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
    public abstract class OverlayArea : IOverlayWindowArea
    {
        internal OverlayArea(IOverlayWindow overlayWindow)
        {
            _overlayWindow = overlayWindow;
        }

        IOverlayWindow _overlayWindow;

        Rect? _screenDetectionArea;
        Rect IOverlayWindowArea.ScreenDetectionArea
        {
            get
            {
                return _screenDetectionArea.Value;
            }
        }

        protected void SetScreenDetectionArea(Rect rect)
        {
            _screenDetectionArea = rect;
        }




    }
}
