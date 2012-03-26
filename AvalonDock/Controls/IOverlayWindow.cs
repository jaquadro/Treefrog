using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvalonDock.Controls
{
    internal interface IOverlayWindow
    {
        IEnumerable<IDropTarget> GetTargets();

        void DragEnter(LayoutFloatingWindowControl floatingWindow);
        void DragLeave(LayoutFloatingWindowControl floatingWindow);

        void DragEnter(IDropArea area);
        void DragLeave(IDropArea area);

        void DragEnter(IDropTarget target);
        void DragLeave(IDropTarget target);
        void DragDrop(IDropTarget target);
    }
}
