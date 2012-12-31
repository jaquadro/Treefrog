using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Presentation.Layers;

namespace Treefrog.Windows.Layers
{
    public class LayerFactory
    {
        private static Dictionary<Type, Type> _registry = new Dictionary<Type, Type>();
        private static Dictionary<Type, Func<CanvasLayer>> _activation1 = new Dictionary<Type, Func<CanvasLayer>>();
        private static Dictionary<Type, Func<LayerPresenter, CanvasLayer>> _activation2 = new Dictionary<Type, Func<LayerPresenter, CanvasLayer>>();

        private static CanvasLayer DefaultActivation<T> ()
        {
            return Activator.CreateInstance(typeof(T)) as CanvasLayer;
        }

        public static CanvasLayer Create (Type layerType)
        {
            Type t;
            if (!_registry.TryGetValue(layerType, out t)) {
                return null;
            }

            if (_activation1.ContainsKey(layerType))
                return _activation1[layerType]();
            else
                return Activator.CreateInstance(t) as CanvasLayer;
        }

        public static CanvasLayer Create (LayerPresenter layer)
        {
            Type t;
            if (!_registry.TryGetValue(layer.GetType(), out t)) {
                return null;
            }

            if (_activation2.ContainsKey(layer.GetType()))
                return _activation2[layer.GetType()](layer);
            else if (_activation1.ContainsKey(layer.GetType()))
                return _activation1[layer.GetType()]();
            else
                return Activator.CreateInstance(t) as CanvasLayer;
        }

        public static Type Lookup (Type layerType)
        {
            Type t;
            if (!_registry.TryGetValue(layerType, out t)) {
                return null;
            }

            return t;
        }

        public static void Register<TKey, T> ()
        {
            Register<TKey, T>(DefaultActivation<T>);
        }

        public static void Register<TKey, T> (Func<CanvasLayer> activation)
        {
            _registry[typeof(TKey)] = typeof(T);
            _activation1[typeof(TKey)] = activation;
        }

        public static void Register<TKey, T> (Func<LayerPresenter, CanvasLayer> activation)
        {
            _registry[typeof(TKey)] = typeof(T);
            _activation2[typeof(TKey)] = activation;
        }

        public static void Register (Type layerType, Type controlType, Func<CanvasLayer> activation)
        {
            _registry[layerType] = controlType;
            _activation1[layerType] = activation;
        }

        public static void Register (Type layerType, Type controlType, Func<LayerPresenter, CanvasLayer> activation)
        {
            _registry[layerType] = controlType;
            _activation2[layerType] = activation;
        }

        static LayerFactory ()
        {
            Register<LayerPresenter, CanvasLayer>();
            Register<WorkspaceLayerPresenter, WorkspaceLayer>();
            Register<GroupLayerPresenter, GroupLayer>(layer => {
                return new GroupLayer() { Model = layer as GroupLayerPresenter };
            });
            Register<LevelLayerPresenter, RenderLayer>(layer => {
                return new RenderLayer() { Model = layer as LevelLayerPresenter };
            });
            Register<TileLayerPresenter, RenderLayer>(layer => {
                return new RenderLayer() { Model = layer as LevelLayerPresenter };
            });
            Register<TileSetLayerPresenter, TileSetLayer>(layer => {
                return new TileSetLayer() { Model = layer as TileSetLayerPresenter };
            });
            Register<TileGridLayerPresenter, RenderLayer>(layer => {
                return new RenderLayer() { Model = layer as LevelLayerPresenter };
            });
            Register<ObjectLayerPresenter, RenderLayer>(layer => {
                return new RenderLayer() { Model = layer as LevelLayerPresenter };
            });
            Register<AnnotationLayerPresenter, AnnotationLayer>(layer => {
                return new AnnotationLayer() { Model = layer as AnnotationLayerPresenter };
            });
            Register<GridLayerPresenter, GridLayer>(layer => {
                return new GridLayer() { Model = layer as GridLayerPresenter };
            });
        }
    }
}
