using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.Presentation.Layers
{
    public abstract class LayerPresenter : IDisposable
    {
        private bool _disposed;

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
    }
}
