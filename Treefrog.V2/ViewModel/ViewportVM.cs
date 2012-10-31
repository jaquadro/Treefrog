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
        private Size _limit;
        private float _zoomFactor = 1f;

        /// <summary>
        /// The offset of the viewport in game units from origin.
        /// </summary>
        public Vector Offset
        {
            get { return _offset; }
            set 
            {
                if (_offset != value) {
                    _offset = value;
                    RaisePropertyChanged("Offset");
                }            
            }
        }

        /// <summary>
        /// The size of the viewable area at 1:1 zoom level.
        /// </summary>
        public Size Viewport
        {
            get { return _viewport; }
            set { _viewport = value; }
        }

        // TODO: May belong in a higher-level view construct.
        /// <summary>
        /// Gets or sets the right/bottom limits of a valid viewport.
        /// </summary>
        public Size Limit
        {
            get { return _limit; }
            set { _limit = value; }
        }

        /// <summary>
        /// The current zoom factor on the viewport.
        /// </summary>
        public float ZoomFactor
        {
            get { return _zoomFactor; }
            set { _zoomFactor = value; }
        }

        /// <summary>
        /// The actual visible area in game units with zoom factor taken into account.
        /// </summary>
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
