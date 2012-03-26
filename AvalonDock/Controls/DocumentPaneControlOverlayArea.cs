using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace AvalonDock.Controls
{
    public class DocumentPaneControlOverlayArea : OverlayArea
    {


        internal DocumentPaneControlOverlayArea(
            IOverlayWindow overlayWindow,
            LayoutDocumentPaneControl documentPaneControl)
            : base(overlayWindow)
        {
            _documentPaneControl = documentPaneControl;
            base.SetScreenDetectionArea(new Rect(
                _documentPaneControl.PointToScreenDPI(new Point()),
                _documentPaneControl.TransformActualSizeToAncestor()));
        }

        LayoutDocumentPaneControl _documentPaneControl;

       
    }
}
