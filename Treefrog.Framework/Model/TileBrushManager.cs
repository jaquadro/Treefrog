using System.Collections.Generic;
using System.Xml.Serialization;
using Treefrog.Framework.Compat;
using Treefrog.Framework.Model.Proxy;
using System;

namespace Treefrog.Framework.Model
{
    public interface ITileBrushManager : IPoolManager<TileBrushCollection>
    {
        TileBrushCollection<StaticTileBrush> DefaultStaticBrushCollection { get; }
        TileBrushCollection<DynamicTileBrush> DefaultDynamicBrushCollection { get; }

        IEnumerable<TileBrushCollection<StaticTileBrush>> StaticBrushCollections { get; }
        IEnumerable<TileBrushCollection<DynamicTileBrush>> DynamicBrushCollections { get; }
        IEnumerable<TileBrush> Brushes { get; }

        TileBrush GetBrush (Guid key);
    }

    public class TileBrushManager : PoolManager<TileBrushCollection, TileBrush>, IPoolManager<TileBrushCollection>, ITileBrushManager
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

        public IEnumerable<TileBrushCollection<StaticTileBrush>> StaticBrushCollections
        {
            get { yield return _staticBrushCollection; }
        }

        public IEnumerable<TileBrushCollection<DynamicTileBrush>> DynamicBrushCollections
        {
            get { yield return _dynamicBrushCollection; }
        }

        public TileBrushCollection<StaticTileBrush> StaticBrushes
        {
            get { return _staticBrushCollection; }
        }

        public TileBrushCollection<DynamicTileBrush> DynamicBrushes
        {
            get { return _dynamicBrushCollection; }
        }

        public TileBrushCollection<StaticTileBrush> DefaultStaticBrushCollection
        {
            get { return _staticBrushCollection; }
        }

        public TileBrushCollection<DynamicTileBrush> DefaultDynamicBrushCollection
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
                StaticBrushes = TileBrushCollection<StaticTileBrush>.ToXProxy<LibraryX.StaticTileBrushX>(manager.StaticBrushes, StaticTileBrush.ToXProxy),
                DynamicBrushes = TileBrushCollection<DynamicTileBrush>.ToXProxy<LibraryX.DynamicTileBrushX>(manager.DynamicBrushes, DynamicTileBrush.ToXProxy),
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
        public IEnumerable<TileBrushCollection<StaticTileBrush>> StaticBrushCollections
        {
            get 
            {
                foreach (var item in Managers)
                    yield return item.StaticBrushes;
            }
        }

        public IEnumerable<TileBrushCollection<DynamicTileBrush>> DynamicBrushCollections
        {
            get
            {
                foreach (var item in Managers)
                    yield return item.DynamicBrushes;
            }
        }

        public IEnumerable<TileBrush> Brushes
        {
            get 
            {
                foreach (var manager in Managers) {
                    foreach (var brush in manager.Brushes)
                        yield return brush;
                }
            }
        }

        public TileBrush GetBrush (Guid key)
        {
            foreach (var manager in Managers) {
                TileBrush brush = manager.GetBrush(key);
                if (brush != null)
                    return brush;
            }
            return null;
        }

        public TileBrushCollection<StaticTileBrush> DefaultStaticBrushCollection
        {
            get { return GetManager(Default).StaticBrushes; }
        }

        public TileBrushCollection<DynamicTileBrush> DefaultDynamicBrushCollection
        {
            get { return GetManager(Default).DynamicBrushes; }
        }
    }
}
