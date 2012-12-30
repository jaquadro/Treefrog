using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Windows.Controls;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Presentation;

namespace Treefrog.Windows.Layers
{
    public class CanvasLayer : IDisposable
    {
        private bool _disposed = false;

        public void Dispose ()
        {
            if (!_disposed) {
                DisposeManaged();
                DisposeUnmanaged();

                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        ~CanvasLayer ()
        {
            DisposeUnmanaged();
        }

        protected virtual void DisposeManaged ()
        {
            //GraphicsDeviceControl = null;
        }

        protected virtual void DisposeUnmanaged ()
        { }

        public CanvasLayer ParentLayer { get; set; }

        private bool _isRendered;

        public bool IsRendered
        {
            get
            {
                if (ParentLayer != null)
                    return ParentLayer.IsRendered;
                else
                    return _isRendered;
            }
            set { _isRendered = value; }
        }

        //public GraphicsDeviceControl GraphicsDeviceControl { get; set; }

        private ILevelGeometry _levelGeometry;

        public ILevelGeometry LevelGeometry
        {
            get 
            {
                if (ParentLayer != null)
                    return ParentLayer.LevelGeometry;
                else
                    return _levelGeometry;
            }
            set { _levelGeometry = value; }
        }

        private TextureCache _textureCache;

        public TextureCache TextureCache
        {
            get
            {
                if (ParentLayer != null)
                    return ParentLayer.TextureCache;
                else
                    return _textureCache;
            }
            set { _textureCache = value; }
        }

        private bool _isLoaded;

        public void Load (GraphicsDevice device)
        {
            LoadCore(device);
            _isLoaded = true;
        }

        public void Render (GraphicsDevice device)
        {
            if (!_isLoaded)
                Load(device);

            if (IsRendered)
                RenderCore(device);
        }

        protected virtual void LoadCore (GraphicsDevice device)
        { }

        protected virtual void RenderCore (GraphicsDevice device)
        { }
    }
}
