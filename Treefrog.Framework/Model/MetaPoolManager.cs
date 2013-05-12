using System;
using System.Collections.Generic;

namespace Treefrog.Framework.Model
{
    public abstract class MetaPoolManager<TPool, TPoolItem, TSubType> : PoolManager<TPool, TPoolItem>
        where TPool : class, IResource, IResourceManager<TPoolItem>
        where TPoolItem : IResource
        where TSubType : PoolManager<TPool, TPoolItem>
    {
        private Guid _default;
        private Dictionary<Guid, TSubType> _managers;

        protected MetaPoolManager ()
        {
            _managers = new Dictionary<Guid, TSubType>();
        }

        public TSubType GetManager (Guid libraryUid)
        {
            return _managers[MapAndCheckUid(libraryUid)];
        }

        public void AddManager (Guid libraryUid, TSubType manager)
        {
            if (libraryUid == Guid.Empty)
                throw new ArgumentException("Library UID must be non-empty.");
            if (_managers.ContainsKey(libraryUid))
                throw new ArgumentException("A manager with the given UID has already been added.");

            _managers.Add(libraryUid, manager);
            if (_managers.Count == 1)
                _default = libraryUid;

            manager.PoolAdded += HandlePoolAdded;
            manager.PoolRemoved += HandlePoolRemoved;
        }

        public bool RemoveManager (Guid libraryUid)
        {
            if (_default == libraryUid)
                _default = Guid.Empty;

            TSubType manager;
            if (_managers.TryGetValue(libraryUid, out manager)) {
                manager.PoolAdded -= HandlePoolAdded;
                manager.PoolRemoved -= HandlePoolRemoved;
            }

            return _managers.Remove(libraryUid);
        }

        public Guid Default
        {
            get { return _default; }
            set
            {
                if (!_managers.ContainsKey(value))
                    throw new ArgumentException("Can only set default library UID to a value that has been previously added.");
                _default = value;
            }
        }

        public override ResourceCollection<TPool> Pools
        {
            get { return _managers[MapAndCheckUid(_default)].Pools; }
        }

        public override void Reset ()
        {
            _managers[MapAndCheckUid(_default)].Reset();
        }

        public override TPool PoolFromItemKey (Guid key)
        {
            return _managers[MapAndCheckUid(_default)].PoolFromItemKey(key);
        }

        public override bool Contains (Guid key)
        {
            return _managers[MapAndCheckUid(_default)].Contains(key);
        }

        private void HandlePoolAdded (object sender, ResourceEventArgs<TPool> e)
        {
            OnPoolAdded(e.Resource);
        }

        private void HandlePoolRemoved (object sender, ResourceEventArgs<TPool> e)
        {
            OnPoolRemoved(e.Resource);
        }

        private void HandlePoolModified (object sender, ResourceEventArgs<TPool> e)
        {
            OnPoolModified(e.Resource);
        }

        private Guid MapAndCheckUid (Guid libraryUid)
        {
            if (libraryUid == Guid.Empty)
                libraryUid = _default;
            if (libraryUid == Guid.Empty)
                throw new InvalidOperationException("No default library has been set for this manager.");
            if (!_managers.ContainsKey(libraryUid))
                throw new ArgumentException("The specified library has not been registered with this manager.");

            return libraryUid;
        }
    }
}
