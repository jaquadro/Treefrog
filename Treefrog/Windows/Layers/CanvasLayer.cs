using System;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Presentation;
using Treefrog.Presentation.Layers;

namespace Treefrog.Windows.Layers
{
    public enum CanvasLayerOption
    {
        Default,
        Always,
        Never,
    }

    public class CanvasLayer : IDisposable
    {
        private bool _disposed = false;
        private LayerPresenter _model;

        public CanvasLayer ()
            : this(null)
        { }

        public CanvasLayer (LayerPresenter model)
        {
            _model = model;
        }

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
        { }

        protected virtual void DisposeUnmanaged ()
        { }

        public CanvasLayer ParentLayer { get; set; }

        protected LayerPresenter Model
        {
            get { return ModelCore; }
        }

        protected virtual LayerPresenter ModelCore
        {
            get { return _model; }
        }

        public bool IsRendered
        {
            get
            {
                switch (IsRenderedOption) {
                    case CanvasLayerOption.Always:
                        return true;
                    case CanvasLayerOption.Never:
                        return false;
                    default:
                        if (ParentLayer != null) {
                            if (Model != null)
                                return ParentLayer.IsRendered && Model.IsVisible;
                            else
                                return ParentLayer.IsRendered;
                        }
                        else if (Model != null)
                            return Model.IsVisible;
                        else
                            return true;
                }
            }
        }

        public CanvasLayerOption IsRenderedOption { get; set; }

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

        public virtual int DependentWidth
        {
            get { return 0; }
        }

        public virtual int DependentHeight
        {
            get { return 0; }
        }
    }
}
