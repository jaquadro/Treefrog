using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace Treefrog.ViewModel.Tools
{
    public abstract class PointerTool : IDisposable
    {
        public void StartPointerSequence (PointerEventInfo info, ViewportVM viewport)
        {
            if (IsCancelled)
                return;
            StartPointerSequenceCore(info, viewport);
        }

        public void UpdatePointerSequence (PointerEventInfo info, ViewportVM viewport)
        {
            if (IsCancelled)
                return;
            UpdatePointerSequenceCore(info, viewport);
        }

        public void EndPointerSequence (PointerEventInfo info, ViewportVM viewport)
        {
            if (IsCancelled)
                return;
            EndPointerSequenceCore(info, viewport);
        }

        public void PointerPosition (PointerEventInfo info, ViewportVM viewport)
        {
            if (IsCancelled)
                return;
            PointerPositionCore(info, viewport);
        }

        public void PointerEnterField ()
        {
            if (IsCancelled)
                return;
            PointerEnterFieldCore();
        }

        public void PointerLeaveField ()
        {
            if (IsCancelled)
                return;
            PointerLeaveFieldCore();
        }

        protected virtual void StartPointerSequenceCore (PointerEventInfo info, ViewportVM viewport)
        {
        }

        protected virtual void UpdatePointerSequenceCore (PointerEventInfo info, ViewportVM viewport)
        {
        }

        protected virtual void EndPointerSequenceCore (PointerEventInfo info, ViewportVM viewport)
        {
        }

        protected virtual void PointerPositionCore (PointerEventInfo info, ViewportVM viewport)
        {
        }

        protected virtual void PointerEnterFieldCore ()
        {
        }

        protected virtual void PointerLeaveFieldCore ()
        {
        }

        private Timer _scrollTimer = new Timer();
        private ViewportVM _scrollViewport;

        private double _prevX;
        private double _prevY;
        private double _xRate;
        private double _yRate;

        protected void StartAutoScroll (PointerEventInfo info, ViewportVM viewport)
        {
            _prevX = info.X;
            _prevY = info.Y;
            _scrollViewport = viewport;

            if (_scrollTimer.Interval != 50) {
                _scrollTimer.Interval = 50;
                _scrollTimer.Tick += new System.EventHandler(ScrollTick);
            }
            _scrollTimer.Enabled = true;
        }

        protected void UpdateAutoScroll (PointerEventInfo info, ViewportVM viewport)
        {
            double deltaX = info.X - _prevX;
            double deltaY = info.Y - _prevY;

            double diffLeft = info.X - viewport.VisibleRegion.Left;
            double diffRight = viewport.VisibleRegion.Right - info.X;
            double diffTop = info.Y - viewport.VisibleRegion.Top;
            double diffBottom = viewport.VisibleRegion.Bottom - info.Y;

            double threshold = 50 / viewport.ZoomFactor;
            double multiplier = 5;

            _xRate = 0;
            _yRate = 0;

            if (diffLeft < threshold) {
                _xRate = multiplier * (diffLeft - threshold) / threshold;
            }

            if (diffRight < threshold) {
                _xRate = multiplier * (threshold - diffRight) / threshold;
            }

            if (diffTop < threshold) {
                _yRate = multiplier * (diffTop - threshold) / threshold;
            }

            if (diffBottom < threshold) {
                _yRate = multiplier * (threshold - diffBottom) / threshold;
            }

            _prevX = info.X;
            _prevY = info.Y;
            _scrollViewport = viewport;
        }

        protected void EndAutoScroll (PointerEventInfo info, ViewportVM viewport)
        {
            _scrollViewport = null;
            _scrollTimer.Enabled = false;
        }

        private void ScrollTick (object sender, EventArgs e)
        {
            if (_scrollViewport != null)
                _scrollViewport.Offset = new Vector(_scrollViewport.Offset.X + _xRate, _scrollViewport.Offset.Y + _yRate);
        }

        public bool IsCancelled
        {
            get { return _cancelled; }
        }

        public void Cancel ()
        {
            Dispose();
        }

        #region Disposable

        private bool _cancelled;

        public void Dispose ()
        {
            if (!_cancelled) {
                DisposeManaged();
                _cancelled = true;
            }
        }

        protected virtual void DisposeManaged ()
        {
        }

        #endregion
    }
}
