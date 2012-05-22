using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.ViewModel.Tools
{
    public abstract class PointerTool : IDisposable
    {
        public void StartPointerSequence (PointerEventInfo info)
        {
            if (IsCancelled)
                return;
            StartPointerSequenceCore(info);
        }

        public void UpdatePointerSequence (PointerEventInfo info)
        {
            if (IsCancelled)
                return;
            UpdatePointerSequenceCore(info);
        }

        public void EndPointerSequence (PointerEventInfo info)
        {
            if (IsCancelled)
                return;
            EndPointerSequenceCore(info);
        }

        public void PointerPosition (PointerEventInfo info)
        {
            if (IsCancelled)
                return;
            PointerPositionCore(info);
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

        protected virtual void StartPointerSequenceCore (PointerEventInfo info)
        {
        }

        protected virtual void UpdatePointerSequenceCore (PointerEventInfo info)
        {
        }

        protected virtual void EndPointerSequenceCore (PointerEventInfo info)
        {
        }

        protected virtual void PointerPositionCore (PointerEventInfo info)
        {
        }

        protected virtual void PointerEnterFieldCore ()
        {
        }

        protected virtual void PointerLeaveFieldCore ()
        {
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
