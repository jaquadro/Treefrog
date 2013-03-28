using System.Collections.Generic;
using System.Xml.Serialization;
using Treefrog.Framework.Compat;
using Treefrog.Framework.Model.Proxy;
using System;

namespace Treefrog.Framework.Model
{
    public interface ITileBrushManager : IPoolManager<TileBrushCollection>
    {
        TileBrushCollection<StaticTileBrush> StaticBrushes { get; }
        TileBrushCollection<DynamicTileBrush> DynamicBrushes { get; }
        IEnumerable<TileBrush> Brushes { get; }

        TileBrush GetBrush (Guid key);
    }

    public class TileBrushManager : PoolManager<TileBrushCollection, TileBrush>, IPoolManager<TileBrushCollection>
    {
        private TileBrushCollection<StaticTileBrush> _staticBrushCollection;
        private TileBrushCollection<DynamicTileBrush> _dynamicBrushCollection;

        public TileBrushManager ()
        {
            _staticBrushCollection = new TileBrushCollection<StaticTileBrush>("Static Brushes");
            _dynamicBrushCollection = new TileBrushCollection<DynamicTileBrush>("Dynamic Brushes");

            Pools.Add(_staticBrushCollection);
            Pools.Add(_dynamicBrushCollection);
        }

        private TileBrushManager (LibraryX.TileBrushGroupX proxy, TilePoolManager tileManager, DynamicTileBrushClassRegistry registry)
        {
            _staticBrushCollection = StaticTileBrushCollection.FromXProxy(proxy.StaticBrushes, tileManager);
            _dynamicBrushCollection = DynamicTileBrushCollection.FromXProxy(proxy.DynamicBrushes, tileManager, registry);

            Pools.Add(_staticBrushCollection);
            Pools.Add(_dynamicBrushCollection);
        }

        public TileBrushCollection<StaticTileBrush> StaticBrushes
        {
            get { return _staticBrushCollection; }
        }

        public TileBrushCollection<DynamicTileBrush> DynamicBrushes
        {
            get { return _dynamicBrushCollection; }
        }

        public IEnumerable<TileBrush> Brushes
        {
            get
            {
                foreach (Guid uid in Keys)
                    yield return GetBrush(uid);
            }
        }

        public TileBrush GetBrush (Guid key)
        {
            TileBrushCollection collection = PoolFromItemKey(key);
            if (collection == null)
                return null;

            return collection.GetBrush(key);
        }

        public static LibraryX.TileBrushGroupX ToXProxy (TileBrushManager manager)
        {
            if (manager == null)
                return null;

            return new LibraryX.TileBrushGroupX() {
                StaticBrushes = TileBrushCollection<StaticTileBrush>.ToXProxy<LibraryX.StaticTileBrushX>(manager.StaticBrushes, StaticTileBrush.ToXmlProxyX),
                DynamicBrushes = TileBrushCollection<DynamicTileBrush>.ToXProxy<LibraryX.DynamicTileBrushX>(manager.DynamicBrushes, DynamicTileBrush.ToXmlProxyX),
            };
        }

        public static TileBrushManager FromXProxy (LibraryX.TileBrushGroupX proxy, TilePoolManager tileManager, DynamicTileBrushClassRegistry registry)
        {
            if (proxy == null)
                return null;

            return new TileBrushManager(proxy, tileManager, registry);
        }
    }

    public class MetaTileBrushManager : MetaPoolManager<TileBrushCollection, TileBrush, TileBrushManager>, ITileBrushManager
    {
        public TileBrushCollection<StaticTileBrush> StaticBrushes
        {
            get { return GetManager(Default).StaticBrushes; }
        }

        public TileBrushCollection<DynamicTileBrush> DynamicBrushes
        {
            get { return GetManager(Default).DynamicBrushes; }
        }

        public IEnumerable<TileBrush> Brushes
        {
            get { return GetManager(Default).Brushes; }
        }

        public TileBrush GetBrush (Guid key)
        {
            return GetManager(Default).GetBrush(key);
        }
    }
}
