using System;
using System.Collections.Generic;
using Treefrog.Framework.Compat;

namespace Treefrog.Framework.Model
{
    public interface IPoolManager<TPool, TItemKey>
        where TPool : class, IResource
    {
        ResourceCollection<TPool> Pools { get; }
        TPool CreatePool (string name);
        void Reset ();
        TPool PoolFromItemKey (TItemKey key);
    }

    public abstract class PoolManager<TPool, TItemKey> : IPoolManager<TPool, TItemKey>
        where TPool : class, IResource
    {
        private ResourceCollection<TPool> _pools;
        private Dictionary<TItemKey, TPool> _poolIndexMap;

        protected PoolManager ()
        {
            _pools = new ResourceCollection<TPool>();
            _poolIndexMap = new Dictionary<TItemKey, TPool>();

            _pools.ResourceAdded += HandleResourceAdded;
            _pools.ResourceRemoved += HandleResourceRemoved;
        }

        private void HandleResourceRemoved (object sender, ResourceEventArgs<TPool> e)
        {
            List<TItemKey> removeQueue = new List<TItemKey>();
            foreach (var item in _poolIndexMap) {
                if (item.Value == e.Resource)
                    removeQueue.Add(item.Key);
            }

            foreach (TItemKey key in removeQueue)
                _poolIndexMap.Remove(key);
        }

        private void HandleResourceAdded (object sender, ResourceEventArgs<TPool> e)
        {
            // Would be nice to map pool items wouldn't it?
        }

        public virtual ResourceCollection<TPool> Pools
        {
            get { return _pools; }
        }

        public virtual TPool CreatePool (string name)
        {
            //if (_pools.Contains(name))
            //    throw new ArgumentException("Manager already contains a pool with the given name.", "name");

            TPool pool = CreatePoolCore(name);
            _pools.Add(pool);

            return pool;
        }

        protected abstract TPool CreatePoolCore (string name);

        public virtual void Reset ()
        {
            _pools.Clear();
            _poolIndexMap.Clear();
        }

        public virtual TPool PoolFromItemKey (TItemKey key)
        {
            TPool item;
            if (_poolIndexMap.TryGetValue(key, out item))
                return item;
            return null;
        }

        internal abstract TItemKey TakeKey ();

        internal virtual void LinkItemKey (TItemKey key, TPool pool)
        {
            _poolIndexMap[key] = pool;
        }

        internal virtual void UnlinkItemKey (TItemKey key)
        {
            _poolIndexMap.Remove(key);
        }
    }
}
