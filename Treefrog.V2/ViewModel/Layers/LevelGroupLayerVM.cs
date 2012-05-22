using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using System.Collections.ObjectModel;
using Treefrog.ViewModel.Utility;

namespace Treefrog.ViewModel.Layers
{
    public class LevelGroupLayerVM : GroupLayerVM
    {
        private LevelDocumentVM _level;
        private OrderedResourceCollection<Layer> _model;
        private ViewportVM _viewport;

        private OrderedToObservableAdapter<Layer, LayerVM> _adapter;

        public LevelGroupLayerVM (LevelDocumentVM level, OrderedResourceCollection<Layer> model, ViewportVM viewport)
        {
            _level = level;
            _model = model;
            _viewport = viewport;

            _adapter = new OrderedToObservableAdapter<Layer, LayerVM>(layer =>
            {
                if (layer is TileGridLayer)
                    return new TileLayerVM(_level, layer as TileGridLayer, _viewport);
                else if (layer is ObjectLayer)
                    return new ObjectLayerVM(_level, layer as ObjectLayer, _viewport);
                else
                    return new LevelLayerVM(_level, layer, _viewport);
            }, model);

            //_model.ResourceAdded += HandleModelLayerAdded;
            //_model.ResourceRemoved += HandleModelLayerRemoved;

            /*if (model != null) {
                foreach (Layer layer in model) {
                    if (layer is TileGridLayer)
                        Layers.Add(new TileLayerVM(layer as TileGridLayer, _viewport));
                    else
                        Layers.Add(new LevelLayerVM(layer, _viewport));
                }
            }*/
        }

        public LevelGroupLayerVM (LevelDocumentVM level, OrderedResourceCollection<Layer> model)
            : this(level, model, null)
        {
        }

        private void HandleModelLayerAdded (object sender, NamedResourceEventArgs<Layer> e)
        {
            Layers.Add(new LevelLayerVM(_level, e.Resource, null));
        }

        private void HandleModelLayerRemoved (object sender, NamedResourceEventArgs<Layer> e)
        {
            foreach (LayerVM layer in Layers) {
                if (layer.LayerName == e.Resource.Name) {
                    Layers.Remove(layer);
                    break;
                }
            }
        }

        private void HandleModelLayerReordered (object sender, OrderedResourceEventArgs<Layer> e)
        {
        }

        public override ObservableCollection<LayerVM> Layers
        {
            get { return _adapter.Dependent; }
        }
    }
}
