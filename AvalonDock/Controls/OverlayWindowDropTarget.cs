using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace AvalonDock.Controls
{
    public class OverlayWindowDropTarget : IOverlayWindowDropTarget
    {
        internal OverlayWindowDropTarget(IOverlayWindowArea overlayArea, OverlayWindowDropTargetType targetType, FrameworkElement element)
        {
            _overlayArea = overlayArea;
            _type = targetType;
            _screenDetectionArea = new Rect(element.TransformToDeviceDPI(new Point()), element.TransformActualSizeToAncestor());
        }

        IOverlayWindowArea _overlayArea;

        Rect _screenDetectionArea;
        Rect IOverlayWindowDropTarget.ScreenDetectionArea
        {
            get
            {
                return _screenDetectionArea;
            }
            
        }

        OverlayWindowDropTargetType _type;
        OverlayWindowDropTargetType IOverlayWindowDropTarget.Type
        {
            get { return _type; }
        }


    }
}
