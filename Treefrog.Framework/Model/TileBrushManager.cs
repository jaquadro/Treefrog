using System.Collections.Generic;
using System.Xml.Serialization;
using Treefrog.Framework.Compat;
using Treefrog.Framework.Model.Proxy;
using System;

namespace Treefrog.Framework.Model
{
    public class TileBrushManager
    {
        private int _lastId;
        private TileBrushCollection<StaticTileBrush> _staticBrushCollection;
        private TileBrushCollection<DynamicTileBrush> _dynamicBrushCollection;

        private Dictionary<int, TileBrushCollection> _indexMap;

        public TileBrushManager ()
        {
            _lastId = 0;
            _staticBrushCollection = new TileBrushCollection<StaticTileBrush>("Static Brushes", this);
            _dynamicBrushCollection = new TileBrushCollection<DynamicTileBrush>("Dynamic Brushes", this);
            _indexMap = new Dictionary<int, TileBrushCollection>();
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
            _lastId = 0;
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

        public TileBrush GetBrush (int key)
        {
            TileBrushCollection collection;
            if (!_indexMap.TryGetValue(key, out collection))
                return null;

            return collection.GetBrush(key);
        }

        internal int LastKey
        {
            get { return _lastId; }
            set { _lastId = value; }
        }

        internal int TakeKey ()
        {
            LastKey++;
            return LastKey;
        }

        internal void LinkItemKey (int key, TileBrushCollection collection)
        {
            _indexMap[key] = collection;
        }

        internal void UnlinkItemKey (int key)
        {
            _indexMap.Remove(key);
        }

        public static TileBrushManagerXmlProxy ToXmlProxy (TileBrushManager manager)
        {
            if (manager == null)
                return null;

            return new TileBrushManagerXmlProxy() {
                LastKey = manager.LastKey,
                StaticBrushes = TileBrushCollection<StaticTileBrush>.ToXmlProxy<StaticTileBrushXmlProxy>(manager.StaticBrushes, StaticTileBrush.ToXmlProxy),
                DynamicBrushes = TileBrushCollection<DynamicTileBrush>.ToXmlProxy<DynamicTileBrushXmlProxy>(manager.DynamicBrushes, DynamicTileBrush.ToXmlProxy),
            };
        }

        [Obsolete]
        public static TileBrushManager FromXmlProxy (TileBrushManagerXmlProxy proxy, TilePoolManager tileManager, DynamicTileBrushClassRegistry registry)
        {
            if (proxy == null)
                return null;

            Func<StaticTileBrushXmlProxy, StaticTileBrush> staticBrushFunc = (brushProxy) => {
                return StaticTileBrush.FromXmlProxy(brushProxy, tileManager);
            };

            Func<DynamicTileBrushXmlProxy, DynamicTileBrush> dynamicBrushFunc = (brushProxy) => {
                return DynamicTileBrush.FromXmlProxy(brushProxy, tileManager, registry);
            };

            TileBrushManager manager = new TileBrushManager();
            TileBrushCollection<StaticTileBrush>.FromXmlProxy<StaticTileBrushXmlProxy>(proxy.StaticBrushes, manager.StaticBrushes, staticBrushFunc);
            TileBrushCollection<DynamicTileBrush>.FromXmlProxy<DynamicTileBrushXmlProxy>(proxy.DynamicBrushes, manager.DynamicBrushes, dynamicBrushFunc);

            return manager;
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
