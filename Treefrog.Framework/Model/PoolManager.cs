using System;
using System.Collections.Generic;
using Treefrog.Framework.Compat;

namespace Treefrog.Framework.Model
{
    public interface IPoolManager<TPool, TItemKey>
        where TPool : class, IKeyProvider<string>
    {
        NamedObservableCollection<TPool> Pools { get; }
        TPool CreatePool (string name);
        void Reset ();
        TPool PoolFromItemKey (TItemKey key);
    }

    public abstract class PoolManager<TPool, TItemKey> : IPoolManager<TPool, TItemKey>
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

            _pools.CollectionChanged += PoolCollectionChangedHandler;
        }

        private void PoolCollectionChangedHandler (object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    foreach (TPool pool in e.OldItems)
                        RemovePool(pool);
                    break;
            }
        }

        private void RemovePool (TPool pool)
        {
            List<TItemKey> removeQueue = new List<TItemKey>();
            foreach (var item in _poolIndexMap) {
                if (item.Value == pool)
                    removeQueue.Add(item.Key);
            }

            foreach (TItemKey key in removeQueue)
                _poolIndexMap.Remove(key);
        }

        public virtual NamedObservableCollection<TPool> Pools
        {
            get { return _pools; }
        }

        public virtual TPool CreatePool (string name)
        {
            if (_pools.Contains(name))
                throw new ArgumentException("Manager already contains a pool with the given name.", "name");

            TPool pool = CreatePoolCore(name);
            _pools.Add(pool);

            return pool;
        }

        protected abstract TPool CreatePoolCore (string name);

        public virtual void Reset ()
        {
            _lastId = default(TItemKey);
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

        internal virtual TItemKey LastKey
        {
            get { return _lastId; }
            set { _lastId = value; }
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
