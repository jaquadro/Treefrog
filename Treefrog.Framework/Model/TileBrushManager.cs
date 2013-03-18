using System.Collections.Generic;
using System.Xml.Serialization;
using Treefrog.Framework.Compat;
using Treefrog.Framework.Model.Proxy;
using System;

namespace Treefrog.Framework.Model
{
    public class TileBrushManager
    {
        private TileBrushCollection<StaticTileBrush> _staticBrushCollection;
        private TileBrushCollection<DynamicTileBrush> _dynamicBrushCollection;

        private Dictionary<Guid, TileBrushCollection> _indexMap;

        public TileBrushManager ()
        {
            _staticBrushCollection = new TileBrushCollection<StaticTileBrush>("Static Brushes", this);
            _dynamicBrushCollection = new TileBrushCollection<DynamicTileBrush>("Dynamic Brushes", this);
            _indexMap = new Dictionary<Guid, TileBrushCollection>();
        }

        public TileBrushCollection<StaticTileBrush> StaticBrushes
        {
            get { return _staticBrushCollection; }
        }

        public TileBrushCollection<DynamicTileBrush> DynamicBrushes
        {
            get { return _dynamicBrushCollection; }
        }

        public void Reset ()
        {
            _staticBrushCollection.Brushes.Clear();
            _dynamicBrushCollection.Brushes.Clear();
        }

        public IEnumerable<TileBrush> Brushes
        {
            get
            {
                foreach (var item in _indexMap)
                    yield return item.Value.GetBrush(item.Key);
            }
        }

        public TileBrush GetBrush (Guid key)
        {
            TileBrushCollection collection;
            if (!_indexMap.TryGetValue(key, out collection))
                return null;

            return collection.GetBrush(key);
        }

        internal Guid TakeKey ()
        {
            return Guid.NewGuid();
        }

        internal void LinkItemKey (Guid key, TileBrushCollection collection)
        {
            _indexMap[key] = collection;
        }

        internal void UnlinkItemKey (Guid key)
        {
            _indexMap.Remove(key);
        }

        public static LibraryX.TileBrushGroupX ToXmlProxyX (TileBrushManager manager)
        {
            if (manager == null)
                return null;

            return new LibraryX.TileBrushGroupX() {
                StaticBrushes = TileBrushCollection<StaticTileBrush>.ToXmlProxyX<LibraryX.StaticTileBrushX>(manager.StaticBrushes, StaticTileBrush.ToXmlProxyX),
                DynamicBrushes = TileBrushCollection<DynamicTileBrush>.ToXmlProxyX<LibraryX.DynamicTileBrushX>(manager.DynamicBrushes, DynamicTileBrush.ToXmlProxyX),
            };
        }

        public static TileBrushManager FromXmlProxy (LibraryX.TileBrushGroupX proxy, TilePoolManager tileManager, DynamicTileBrushClassRegistry registry)
        {
            if (proxy == null)
                return null;

            Func<LibraryX.StaticTileBrushX, StaticTileBrush> staticBrushFunc = (brushProxy) => {
                return StaticTileBrush.FromXmlProxy(brushProxy, tileManager);
            };

            Func<LibraryX.DynamicTileBrushX, DynamicTileBrush> dynamicBrushFunc = (brushProxy) => {
                return DynamicTileBrush.FromXmlProxy(brushProxy, tileManager, registry);
            };

            TileBrushManager manager = new TileBrushManager();
            TileBrushCollection<StaticTileBrush>.FromXmlProxy<LibraryX.StaticTileBrushX>(proxy.StaticBrushes, manager.StaticBrushes, staticBrushFunc);
            TileBrushCollection<DynamicTileBrush>.FromXmlProxy<LibraryX.DynamicTileBrushX>(proxy.DynamicBrushes, manager.DynamicBrushes, dynamicBrushFunc);

            return manager;
        }
    }
}
