using System;

namespace Treefrog.Presentation.Layers
{
    public abstract class LayerPresenter : IDisposable
    {
        private bool _disposed;
        private bool _visible = true;

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~LayerPresenter ()
        {
            Dispose(false);
        }

        private void Dispose (bool disposing)
        {
            if (!_disposed) {
                if (disposing)
                    DisposeManaged();
                DisposeUnmanaged();

                _disposed = true;
            }
        }

        protected virtual void DisposeManaged ()
        { }

        protected virtual void DisposeUnmanaged ()
        { }

        public virtual bool IsVisible
        {
            get { return _visible; }
            set
            {
                if (_visible != value) {
                    _visible = value;
                    OnVisibilityChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler VisibilityChanged;

        protected virtual void OnVisibilityChanged (EventArgs e)
        {
            var ev = VisibilityChanged;
            if (ev != null)
                ev(this, e);
        }
    }
}
