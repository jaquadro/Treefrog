using System;
using System.Collections.Generic;

namespace Treefrog.Framework.Model
{
    public abstract class PoolManager<TPool, TItemKey>
        where TPool : class, IKeyProvider<string>
    {
        private TItemKey _lastId;
        private NamedObservableCollection<TPool> _pools;
        private Dictionary<TItemKey, TPool> _poolIndexMap;

        protected PoolManager ()
        {
            _lastId = default(TItemKey);
            _pools = new NamedObservableCollection<TPool>();
            _poolIndexMap = new Dictionary<TItemKey, TPool>();
        }

        public NamedObservableCollection<TPool> Pools
        {
            get { return _pools; }
        }

        public TPool CreatePool (string name)
        {
            if (_pools.Contains(name))
                throw new ArgumentException("Manager already contains a pool with the given name.", "name");

            TPool pool = CreatePoolCore(name);
            _pools.Add(pool);

            return pool;
        }

        protected abstract TPool CreatePoolCore (string name);

        public void Reset ()
        {
            _lastId = default(TItemKey);
            _pools.Clear();
            _poolIndexMap.Clear();
        }

        public TPool PoolFromItemKey (TItemKey key)
        {
            TPool item;
            if (_poolIndexMap.TryGetValue(key, out item))
                return item;
            return null;
        }

        internal TItemKey LastKey
        {
            get { return _lastId; }
            set { _lastId = value; }
        }

        internal abstract TItemKey TakeKey ();

        internal void LinkItemKey (TItemKey key, TPool pool)
        {
            _poolIndexMap[key] = pool;
        }

        internal void UnlinkItemKey (TItemKey key)
        {
            _poolIndexMap.Remove(key);
        }
    }
}
