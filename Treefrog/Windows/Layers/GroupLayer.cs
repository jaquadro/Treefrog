using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Utility;
using Treefrog.Presentation.Layers;
using System.Collections.Specialized;

namespace Treefrog.Windows.Layers
{
    public class GroupLayer : CanvasLayer
    {
        private ObservableCollectionAdapter<LayerPresenter, CanvasLayer> _adapter;
        private ReadOnlyObservableCollection<CanvasLayer> _dependent;

        public GroupLayer ()
        {
            _adapter = new ObservableCollectionAdapter<LayerPresenter, CanvasLayer>(layer => {
                return LayerFactory.Create(layer);
            });
            _adapter.Dependent.CollectionChanged += DependentCollectionChanged;

            _dependent = new ReadOnlyObservableCollection<CanvasLayer>(_adapter.Dependent);
        }

        protected override void DisposeManaged ()
        {
            foreach (CanvasLayer layer in Layers)
                if (layer != null)
                    layer.Dispose();

            _adapter.Dependent.CollectionChanged -= DependentCollectionChanged;
            _adapter.Primary = null;

            base.DisposeManaged();
        }

        private GroupLayerPresenter _model;

        public GroupLayerPresenter Model
        {
            get { return _model; }
            set
            {
                if (_model != value) {
                    foreach (CanvasLayer layer in Layers)
                        if (layer != null)
                            layer.ParentLayer = null;

                    _model = value;
                    if (_model != null)
                        _adapter.Primary = _model.Layers;
                    else
                        _adapter.Primary = null;
                }
            }
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
                    foreach (CanvasLayer layer in e.NewItems)
                        if (layer != null)
                            layer.ParentLayer = this;
                    break;
            }
        }
    }
}
