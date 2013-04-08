using System;
using LilyPath;
using Microsoft.Xna.Framework.Graphics;

namespace Treefrog.Render.Annotations
{
    public abstract class AnnotationRenderer : IDisposable
    {
        public void Render (SpriteBatch spriteBatch)
        {
            Render(spriteBatch, 1f);
        }

        public void Render (DrawBatch drawBatch)
        {
            Render(drawBatch, 1f);
        }

        public virtual void Render (SpriteBatch spriteBatch, float zoomFactor)
        { }

        public virtual void Render (DrawBatch drawBatch, float zoomFactor)
        { }

        #region Dispose

        private bool _disposed = false;

        public void Dispose ()
        {
            if (!_disposed) {
                DisposeManaged();
                DisposeUnamanged();

                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        public bool IsDisposed
        {
            get { return _disposed; }
        }

        protected virtual void DisposeManaged ()
        {
        }

        protected virtual void DisposeUnamanged ()
        {
        }

        #endregion
    }
}
