using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Presentation.Layers;
using Treefrog.Utility;

namespace Treefrog.Render.Layers
{
    public class GroupLayer : CanvasLayer
    {
        private ObservableCollectionAdapter<LayerPresenter, CanvasLayer> _adapter;
        private ReadOnlyObservableCollection<CanvasLayer> _dependent;

        public GroupLayer (GroupLayerPresenter model)
            : base(model)
        {
            _adapter = new ObservableCollectionAdapter<LayerPresenter, CanvasLayer>(layer => {
                return LayerFactory.Default.Create(layer);
            });
            _adapter.Dependent.CollectionChanged += DependentCollectionChanged;

            if (model != null)
                _adapter.Primary = model.Layers;

            _dependent = new ReadOnlyObservableCollection<CanvasLayer>(_adapter.Dependent);
        }

        protected override void DisposeManaged ()
        {
            foreach (CanvasLayer layer in Layers) {
                if (layer != null)
                    layer.Dispose();
            }

            _adapter.Dependent.CollectionChanged -= DependentCollectionChanged;
            _adapter.Primary = null;

            base.DisposeManaged();
        }

        protected new GroupLayerPresenter Model
        {
            get { return ModelCore as GroupLayerPresenter; }
        }

        public ReadOnlyObservableCollection<CanvasLayer> Layers
        {
            get { return _dependent; }
        }

        protected override void LoadCore (GraphicsDevice device)
        {
            foreach (CanvasLayer layer in Layers)
                if (layer != null)
                    layer.Load(device);
        }

        protected override void RenderCore (GraphicsDevice device)
        {
            foreach (CanvasLayer layer in Layers)
                if (layer != null)
                    layer.Render(device);
        }

        private void DependentCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (CanvasLayer layer in e.NewItems) {
                        if (layer != null) {
                            layer.ParentLayer = this;
                            layer.DependentSizeChanged += DependentSizeChangedHandler;
                        }
                    }
                    break;
            }

            OnDependentSizeChanged(EventArgs.Empty);
        }

        public override int DependentHeight
        {
            get
            {
                int height = 0;
                foreach (CanvasLayer layer in Layers)
                    height = Math.Max(height, layer.DependentHeight);
                return height;
            }
        }

        public override int DependentWidth
        {
            get
            {
                int width = 0;
                foreach (CanvasLayer layer in Layers)
                    width = Math.Max(width, layer.DependentWidth);
                return width;
            }
        }

        private void DependentSizeChangedHandler (object sender, EventArgs e)
        {
            OnDependentSizeChanged(EventArgs.Empty);
        }
    }
}
