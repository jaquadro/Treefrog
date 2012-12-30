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
        public void StartPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
        {
            if (IsCancelled)
                return;
            StartPointerSequenceCore(info, viewport);
        }

        public void UpdatePointerSequence (PointerEventInfo info, ILevelGeometry viewport)
        {
            if (IsCancelled)
                return;
            UpdatePointerSequenceCore(info, viewport);
        }

        public void EndPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
        {
            if (IsCancelled)
                return;
            EndPointerSequenceCore(info, viewport);
        }

        public void PointerPosition (PointerEventInfo info, ILevelGeometry viewport)
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

        protected virtual void StartPointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
        }

        protected virtual void UpdatePointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
        }

        protected virtual void EndPointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
        }

        protected virtual void PointerPositionCore (PointerEventInfo info, ILevelGeometry viewport)
        {
        }

        protected virtual void PointerEnterFieldCore ()
        {
        }

        protected virtual void PointerLeaveFieldCore ()
        {
        }

        private Timer _scrollTimer = new Timer();
        private ILevelGeometry _scrollViewport;

        private double _prevX;
        private double _prevY;
        private double _xRate;
        private double _yRate;

        protected void StartAutoScroll (PointerEventInfo info, ILevelGeometry viewport)
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

        protected void UpdateAutoScroll (PointerEventInfo info, ILevelGeometry viewport)
        {
            double deltaX = info.X - _prevX;
            double deltaY = info.Y - _prevY;

            double diffLeft = info.X - viewport.VisibleBounds.Left;
            double diffRight = viewport.VisibleBounds.Right - info.X;
            double diffTop = info.Y - viewport.VisibleBounds.Top;
            double diffBottom = viewport.VisibleBounds.Bottom - info.Y;

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

        protected void EndAutoScroll (PointerEventInfo info, ILevelGeometry viewport)
        {
            _scrollViewport = null;
            _scrollTimer.Enabled = false;
        }

        private void ScrollTick (object sender, EventArgs e)
        {
            if (_scrollViewport != null) {
                if (_scrollViewport.ScrollPosition.X + _xRate < 0)
                    _xRate = Math.Max(0, _scrollViewport.ScrollPosition.X + _xRate);
                if (_scrollViewport.ScrollPosition.Y + _yRate < 0)
                    _yRate = Math.Max(0, _scrollViewport.ScrollPosition.Y + _yRate);

                if (_scrollViewport.VisibleBounds.Right + _xRate >= _scrollViewport.LevelBounds.Width)
                    _xRate = Math.Max(0, Math.Min(_scrollViewport.LevelBounds.Width - _scrollViewport.VisibleBounds.Right, _xRate));
                if (_scrollViewport.VisibleBounds.Bottom + _yRate >= _scrollViewport.LevelBounds.Height)
                    _yRate = Math.Max(0, Math.Min(_scrollViewport.LevelBounds.Height - _scrollViewport.VisibleBounds.Bottom, _yRate));

                //_scrollViewport.Offset = new Point(_scrollViewport.ScrollPosition.X + (int)_xRate, _scrollViewport.ScrollPosition.Y + (int)_yRate);

                _prevX = _prevX + _xRate;
                _prevY = _prevY + _yRate;
                PointerEventInfo info = new PointerEventInfo(PointerEventType.None, _prevX, _prevY);
                AutoScrollTick(info, _scrollViewport);
            }
        }

        protected virtual void AutoScrollTick (PointerEventInfo info, ILevelGeometry viewport)
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
