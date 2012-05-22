using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using GalaSoft.MvvmLight;

namespace Treefrog.ViewModel
{
    public class ViewportVM : ViewModelBase
    {
        private Vector _offset;
        private Size _viewport;
        private float _zoomFactor = 1f;

        public Vector Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        public Size Viewport
        {
            get { return _viewport; }
            set { _viewport = value; }
        }

        public float ZoomFactor
        {
            get { return _zoomFactor; }
            set { _zoomFactor = value; }
        }

        public Rect VisibleRegion
        {
            get
            {
                return new Rect(_offset.X, _offset.Y,
                    _viewport.Width / _zoomFactor, _viewport.Height / _zoomFactor);
            }
        }
    }
}
