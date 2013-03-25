using System;
using System.Collections.Generic;
using Treefrog.Framework.Compat;

namespace Treefrog.Framework.Model
{
    public interface IPoolManager<TPool>
        where TPool : class, IResource
    {
        ResourceCollection<TPool> Pools { get; }
        TPool CreatePool (string name);
        void Reset ();
        TPool PoolFromItemKey (Guid key);
    }

    public abstract class PoolManager<TPool, TPoolItem> : IPoolManager<TPool>
        where TPool : class, IResource, IResourceManager<TPoolItem>
        where TPoolItem : IResource
    {
        private ResourceCollection<TPool> _pools;
        private Dictionary<Guid, EventHandler<ResourceEventArgs<TPoolItem>>> _poolResourceAddHandlers;
        private Dictionary<Guid, EventHandler<ResourceEventArgs<TPoolItem>>> _poolResourceRemoveHandlers;

        private Dictionary<Guid, TPool> _poolIndexMap;

        protected PoolManager ()
        {
            _pools = new ResourceCollection<TPool>();
            _poolResourceAddHandlers = new Dictionary<Guid, EventHandler<ResourceEventArgs<TPoolItem>>>();
            _poolResourceRemoveHandlers = new Dictionary<Guid, EventHandler<ResourceEventArgs<TPoolItem>>>();

            _poolIndexMap = new Dictionary<Guid, TPool>();

            _pools.ResourceAdded += HandleResourceAdded;
            _pools.ResourceRemoved += HandleResourceRemoved;
        }

        private void HandleResourceRemoved (object sender, ResourceEventArgs<TPool> e)
        {
            if (_poolResourceAddHandlers.ContainsKey(e.Uid)) {
                e.Resource.Items.ResourceAdded -= _poolResourceAddHandlers[e.Uid];
                _poolResourceAddHandlers.Remove(e.Uid);
            }

            if (_poolResourceRemoveHandlers.ContainsKey(e.Uid)) {
                e.Resource.Items.ResourceRemoved -= _poolResourceRemoveHandlers[e.Uid];
                _poolResourceRemoveHandlers.Remove(e.Uid);
            }

            List<Guid> removeQueue = new List<Guid>();
            foreach (var item in _poolIndexMap) {
                if (item.Value == e.Resource)
                    removeQueue.Add(item.Key);
            }

            foreach (Guid key in removeQueue)
                _poolIndexMap.Remove(key);
        }

        private void HandleResourceAdded (object sender, ResourceEventArgs<TPool> e)
        {
            _poolResourceAddHandlers[e.Uid] = (s, es) => { _poolIndexMap.Add(es.Uid, e.Resource); };
            _poolResourceRemoveHandlers[e.Uid] = (s, es) => { _poolIndexMap.Remove(es.Uid); };

            e.Resource.Items.ResourceAdded += _poolResourceAddHandlers[e.Uid];
            e.Resource.Items.ResourceRemoved += _poolResourceRemoveHandlers[e.Uid];

            foreach (TPoolItem item in e.Resource.Items)
                _poolIndexMap.Add(item.Uid, e.Resource);
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

            _poolResourceAddHandlers.Clear();
            _poolResourceRemoveHandlers.Clear();
            _poolIndexMap.Clear();
        }

        public virtual TPool PoolFromItemKey (Guid key)
        {
            TPool item;
            if (_poolIndexMap.TryGetValue(key, out item))
                return item;
            return null;
        }
    }
}
