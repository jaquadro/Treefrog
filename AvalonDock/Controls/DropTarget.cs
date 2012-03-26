using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
    internal abstract class DropTarget<T> : IDropTarget where T : FrameworkElement
    {
        protected DropTarget(T targetElement, Rect detectionRect, DropTargetType type)
        {
            _targetElement = targetElement;
            _detectionRect = new Rect[] { detectionRect };
            _type = type;
        }
        
        protected DropTarget(T targetElement, IEnumerable<Rect> detectionRects, DropTargetType type)
        {
            _targetElement = targetElement;
            _detectionRect = detectionRects.ToArray();
            _type = type;
        }

        Rect[] _detectionRect;

        public Rect[] DetectionRects
        {
            get { return _detectionRect; }
        }


        T _targetElement;
        public T TargetElement
        {
            get { return _targetElement; }
        }

        DropTargetType _type;

        public DropTargetType Type
        {
            get { return _type; }
        }

        protected virtual void Drop(LayoutAnchorableFloatingWindow floatingWindow)
        { }

        protected virtual void Drop(LayoutDocumentFloatingWindow floatingWindow)
        { }


        public void Drop(LayoutFloatingWindow floatingWindow)
        {
            var fwAsAnchorable = floatingWindow as LayoutAnchorableFloatingWindow;

            if (fwAsAnchorable != null)
                this.Drop(fwAsAnchorable);
            else
            {
                var fwAsDocument = floatingWindow as LayoutDocumentFloatingWindow;
                this.Drop(fwAsDocument);
            }

        }

        public virtual bool HitTest(Point dragPoint)
        {
            return _detectionRect.Any(dr => dr.Contains(dragPoint));
        }

        public abstract Geometry GetPreviewPath(OverlayWindow overlayWindow, LayoutFloatingWindow floatingWindow);
    }
}
