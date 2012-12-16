using System.Collections.Generic;
using System.Xml.Serialization;
using Treefrog.Framework.Compat;

namespace Treefrog.Framework.Model
{
    [XmlRoot("TileBrushes")]
    public class TileBrushManagerXmlProxy
    {
        [XmlAttribute]
        public int LastKey { get; set; }

        [XmlElement]
        public TileBrushCollectionXmlProxy<DynamicBrushXmlProxy> DynamicBrushes { get; set; }
    }

    public class TileBrushManager
    {
        private int _lastId;
        private TileBrushCollection<DynamicBrush> _dynamicBrushCollection;

        private Dictionary<int, TileBrushCollection> _indexMap;

        public TileBrushManager ()
        {
            _lastId = 0;
            _dynamicBrushCollection = new TileBrushCollection<DynamicBrush>("Dynamic Brushes", this);
            _indexMap = new Dictionary<int, TileBrushCollection>();
        }

        public TileBrushCollection<DynamicBrush> DynamicBrushes
        {
            get { return _dynamicBrushCollection; }
        }

        public void Reset ()
        {
            _lastId = 0;
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
                DynamicBrushes = TileBrushCollection<DynamicBrush>.ToXmlProxy<DynamicBrushXmlProxy>(manager.DynamicBrushes, DynamicBrush.ToXmlProxy),
            };
        }

        public static TileBrushManager FromXmlProxy (TileBrushManagerXmlProxy proxy, TilePoolManager tileManager, DynamicBrushClassRegistry registry)
        {
            if (proxy == null)
                return null;

            Func<DynamicBrushXmlProxy, DynamicBrush> brushFunc = (brushProxy) => {
                return DynamicBrush.FromXmlProxy(brushProxy, tileManager, registry);
            };

            TileBrushManager manager = new TileBrushManager();
            TileBrushCollection<DynamicBrush>.FromXmlProxy<DynamicBrushXmlProxy>(proxy.DynamicBrushes, manager.DynamicBrushes, brushFunc);

            return manager;
        }
    }
}
