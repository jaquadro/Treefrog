using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Editor.Model.Controls;

namespace Editor
{
    public abstract class MouseTool : IDisposable
    {
        private TileControlLayer _control;

        private bool _enabled;

        protected MouseTool (TileControlLayer control)
        {
            _control = control;
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (Disposed) {
                    throw new ObjectDisposedException(GetType().Name);
                }

                if (_enabled && !value) {
                    DetachHandlers();
                }
                else if (!_enabled && value) {
                    AttachHandlers();
                }

                _enabled = value;
            }
        }

        protected bool Disposed { get; private set; }

        protected virtual void AttachHandlers ()
        {
            _control.MouseTileClick += MouseTileClick;
            _control.MouseTileDown += MouseTileDown;
            _control.MouseTileMove += MouseTileMove;
            _control.MouseTileUp += MouseTileUp;
        }

        protected virtual void DetachHandlers ()
        {
            _control.MouseTileClick -= MouseTileClick;
            _control.MouseTileDown -= MouseTileDown;
            _control.MouseTileMove -= MouseTileMove;
            _control.MouseTileUp -= MouseTileUp;
        }

        protected virtual void MouseTileClickHandler (object sender, TileMouseEventArgs e) { }
        protected virtual void MouseTileDownHandler (object sender, TileMouseEventArgs e) { }
        protected virtual void MouseTileUpHandler (object sender, TileMouseEventArgs e) { }
        protected virtual void MouseTileMoveHandler (object sender, TileMouseEventArgs e) { }

        #region Private

        private void MouseTileClick (object sender, TileMouseEventArgs e)
        {
            if (_enabled) {
                MouseTileClickHandler(sender, e);
            }
        }

        private void MouseTileDown (object sender, TileMouseEventArgs e)
        {
            if (_enabled) {
                MouseTileDownHandler(sender, e);
            }
        }

        private void MouseTileMove (object sender, TileMouseEventArgs e)
        {
            if (_enabled) {
                MouseTileMoveHandler(sender, e);
            }
        }

        private void MouseTileUp (object sender, TileMouseEventArgs e)
        {
            if (_enabled) {
                MouseTileUpHandler(sender, e);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (!this.Disposed) {
                if (disposing) {
                    Enabled = false;
                }

                Disposed = true;
            }
        }

        ~MouseTool ()
        {
            Dispose(false);
        }

        #endregion
    }
}
