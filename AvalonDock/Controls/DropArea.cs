using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace AvalonDock.Controls
{
    public enum DropAreaType
    {
        DockingManager,

        DocumentPane,

        DocumentPaneGroup,

        AnchorablePane,

    }


    public interface IDropArea
    {
        Rect DetectionRect { get; }
        DropAreaType Type { get; }
    }

    public class DropArea<T> : IDropArea where T : FrameworkElement
    {
        internal DropArea(T areaElement, DropAreaType type)
        {
            _element = areaElement;
            _detectionRect = areaElement.GetScreenArea();
            _type = type;
        }

        Rect _detectionRect;

        public Rect DetectionRect
        {
            get { return _detectionRect; }
        }

        DropAreaType _type;

        public DropAreaType Type
        {
            get { return _type; }
        }

        T _element;
        public T AreaElement
        {
            get
            {
                return _element;
            }
        }

    }
}
