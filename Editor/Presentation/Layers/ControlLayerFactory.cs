using System;
using System.Collections.Generic;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Model;
using Treefrog.View.Controls;

namespace Treefrog.Presentation.Layers
{
    public class ControlLayerFactory
    {
        private static Dictionary<Type, Type> _registry = new Dictionary<Type, Type>();

        /// <summary>
        /// Create a new instance of a concrete <see cref="BaseControlLayer"/> type from a <see cref="Layer"/> type.
        /// </summary>
        /// <param name="layerType">The name that a concrete <see cref="BaseControlLayer"/> type was registered with.</param>
        /// <param name="control">The <see cref="LayerControl"/> to bind the <see cref="BaseControlLayer"/> instance with.</param>
        /// <returns>A new instance of a concrete <see cref="BaseControlLayer"/> type, or null if no type was registered with the given name.</returns>
        public static BaseControlLayer Create (Type layerType, LayerControl control)
        {
            Type t;
            if (!_registry.TryGetValue(layerType, out t)) {
                return null;
            }

            return Activator.CreateInstance(t, new object[] {control}) as BaseControlLayer;
        }

        /// <summary>
        /// Create a new instance of a concrete <see cref="BaseControlLayer"/> type from a <see cref="Layer"/> instance.
        /// </summary>
        /// <param name="layer">A <see cref="Layer"/> instance.</param>
        /// <param name="control">The <see cref="LayerControl"/> to bind the <see cref="BaseControlLayer"/> instance with.</param>
        /// <returns>A new instance of a concrete <see cref="BaseControlLayer"/> type, or null if no type was registered with the given name.</returns>
        public static BaseControlLayer Create (Layer layer, LayerControl control)
        {
            Type t;
            if (!_registry.TryGetValue(layer.GetType(), out t)) {
                return null;
            }

            return Activator.CreateInstance(t, new object[] { control, layer }) as BaseControlLayer;
        }

        /// <summary>
        /// Lookup a concrete <see cref="BaseControlLayer"/> type.
        /// </summary>
        /// <param name="layerType">The <see cref="Layer"/> type that a concrete <see cref="BaseControlLayer"/> type was registered with.</param>
        /// <returns>The <see cref="Type"/> of a concrete <see cref="BaseControlLayer"/> type, or null if no type was registered with the given <see cref="Layer"/> type.</returns>
        public static Type Lookup (Type layerType)
        {
            Type t;
            if (!_registry.TryGetValue(layerType, out t)) {
                return null;
            }

            return t;
        }

        /// <summary>
        /// Registers a new concrete <see cref="BaseControlLayer"/> type with the <see cref="ControlLayerFactory"/>, binding it to a given <see cref="Layer"/> type.
        /// </summary>
        /// <param name="layerType">The <see cref="Type"/> of a concrete <see cref="Layer"/> to bind to.</param>
        /// <param name="controlType">The <see cref="Type"/> of a concrete <see cref="BaseControlLayer"/> being bound.</param>
        public static void Register (Type layerType, Type controlType)
        {
            _registry[layerType] = controlType;
        }

        static ControlLayerFactory ()
        {
            _registry[typeof(Layer)] = typeof(BaseControlLayer);
            _registry[typeof(TileLayer)] = typeof(TileControlLayer);
            _registry[typeof(TileGridLayer)] = typeof(TileControlLayer);
            _registry[typeof(MultiTileGridLayer)] = typeof(MultiTileControlLayer);
            _registry[typeof(TileSetLayer)] = typeof(TileSetControlLayer);
        }
    }

    /// <summary>
    /// An exception that is thrown when unknown Layer types are queried.
    /// </summary>
    public class UnknownLayerTypeException : Exception
    {
        public UnknownLayerTypeException (string message)
            : base(message)
        { }
    }
}
