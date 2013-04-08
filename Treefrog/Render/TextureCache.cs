using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Aux;
using Treefrog.Framework;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model;

namespace Treefrog.Render
{
    public class TextureCache : IDisposable
    {
        private bool _disposed;
        private GraphicsDevice _device;
        private TexturePool _sourcePool;
        private Dictionary<Guid, Texture2D> _cache;

        public TextureCache ()
        {
            _cache = new Dictionary<Guid, Texture2D>();
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (!_disposed) {
                if (disposing) {
                    ReleaseAll();
                    SourcePool = null;
                    GraphicsDevice = null;
                }

                _disposed = true;
            }
        }

        public TexturePool SourcePool
        {
            get { return _sourcePool; }
            set
            {
                if (_sourcePool != value) {
                    if (_sourcePool != null) {
                        _sourcePool.ResourceInvalidated -= SourcePoolResourceInvalidated;
                    }

                    _sourcePool = value;
                    if (_sourcePool != null) {
                        _sourcePool.ResourceInvalidated += SourcePoolResourceInvalidated;
                    }

                    ReleaseAll();
                }
            }
        }

        public GraphicsDevice GraphicsDevice
        {
            get { return _device; }
            set
            {
                if (_device != value) {
                    if (_device != null) {
                        _device.DeviceLost -= GraphicsDeviceLost;
                    }

                    _device = value;
                    if (_device != null) {
                        _device.DeviceLost += GraphicsDeviceLost;
                    }

                    ReleaseAll();
                }
            }
        }

        public Texture2D Resolve (Guid textureId)
        {
            if (_disposed)
                throw new ObjectDisposedException("TextureCache");

            if (_device == null || _sourcePool == null)
                return null;

            Texture2D tex;
            if (_cache.TryGetValue(textureId, out tex) && tex != null)
                return tex;

            TextureResource resource = _sourcePool.GetResource(textureId);
            if (resource != null) {
                tex = resource.CreateTexture(_device);
                _cache.Add(textureId, tex);
            }

            return tex;
        }

        public void Release (Guid uid)
        {
            Texture2D tex;
            if (_cache.TryGetValue(uid, out tex) && tex != null) {
                tex.Dispose();
                _cache.Remove(uid);
            }
        }

        public void ReleaseAll ()
        {
            try {
                foreach (Texture2D tex in _cache.Values) {
                    if (tex != null)
                        tex.Dispose();
                }
            }
            catch {
                // If dispose fails, shrug it off
            }

            _cache.Clear();
        }

        private void SourcePoolResourceInvalidated (object sender, ResourceEventArgs e)
        {
            Release(e.Uid);
        }

        private void GraphicsDeviceLost (object sender, EventArgs e)
        {
            ReleaseAll();
        }
    }
}
