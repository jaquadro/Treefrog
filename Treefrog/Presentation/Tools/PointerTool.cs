using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Treefrog.Presentation.Tools
{
    public enum PointerEventType
    {
        None,
        Primary,
        Secondary,
    }
    
    public struct PointerEventInfo
    {
        public PointerEventType Type;
        public double X;
        public double Y;

        public PointerEventInfo (PointerEventType type, double x, double y)
        {
            Type = type;
            X = x;
            Y = y;
        }
    }

    public abstract class PointerTool : IDisposable
    {
        public void StartPointerSequence (PointerEventInfo info, IViewport viewport)
        {
            if (IsCancelled)
                return;
            StartPointerSequenceCore(info, viewport);
        }

        public void UpdatePointerSequence (PointerEventInfo info, IViewport viewport)
        {
            if (IsCancelled)
                return;
            UpdatePointerSequenceCore(info, viewport);
        }

        public void EndPointerSequence (PointerEventInfo info, IViewport viewport)
        {
            if (IsCancelled)
                return;
            EndPointerSequenceCore(info, viewport);
        }

        public void PointerPosition (PointerEventInfo info, IViewport viewport)
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

        protected virtual void StartPointerSequenceCore (PointerEventInfo info, IViewport viewport)
        {
        }

        protected virtual void UpdatePointerSequenceCore (PointerEventInfo info, IViewport viewport)
        {
        }

        protected virtual void EndPointerSequenceCore (PointerEventInfo info, IViewport viewport)
        {
        }

        protected virtual void PointerPositionCore (PointerEventInfo info, IViewport viewport)
        {
        }

        protected virtual void PointerEnterFieldCore ()
        {
        }

        protected virtual void PointerLeaveFieldCore ()
        {
        }

        private Timer _scrollTimer = new Timer();
        private IViewport _scrollViewport;

        private double _prevX;
        private double _prevY;
        private double _xRate;
        private double _yRate;

        protected void StartAutoScroll (PointerEventInfo info, IViewport viewport)
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

        protected void UpdateAutoScroll (PointerEventInfo info, IViewport viewport)
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

        protected void EndAutoScroll (PointerEventInfo info, IViewport viewport)
        {
            _scrollViewport = null;
            _scrollTimer.Enabled = false;
        }

        private void ScrollTick (object sender, EventArgs e)
        {
            if (_scrollViewport != null) {
                if (_scrollViewport.Offset.X + _xRate < 0)
                    _xRate = Math.Max(0, _scrollViewport.Offset.X + _xRate);
                if (_scrollViewport.Offset.Y + _yRate < 0)
                    _yRate = Math.Max(0, _scrollViewport.Offset.Y + _yRate);

                if (_scrollViewport.VisibleRegion.Right + _xRate >= _scrollViewport.Limit.Width)
                    _xRate = Math.Max(0, Math.Min(_scrollViewport.Limit.Width - _scrollViewport.VisibleRegion.Right, _xRate));
                if (_scrollViewport.VisibleRegion.Bottom + _yRate >= _scrollViewport.Limit.Height)
                    _yRate = Math.Max(0, Math.Min(_scrollViewport.Limit.Height - _scrollViewport.VisibleRegion.Bottom, _yRate));

                _scrollViewport.Offset = new Point(_scrollViewport.Offset.X + (int)_xRate, _scrollViewport.Offset.Y + (int)_yRate);

                _prevX = _prevX + _xRate;
                _prevY = _prevY + _yRate;
                PointerEventInfo info = new PointerEventInfo(PointerEventType.None, _prevX, _prevY);
                AutoScrollTick(info, _scrollViewport);
            }
        }

        protected virtual void AutoScrollTick (PointerEventInfo info, IViewport viewport)
        {
        }

        #region Disposable

        private bool _cancelled;

        public bool IsCancelled
        {
            get { return _cancelled; }
        }

        public void Cancel ()
        {
            Dispose();
        }

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
