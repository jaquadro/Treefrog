using System;
using System.Collections.Generic;

namespace Treefrog.Framework.Model
{
    public abstract class MetaPoolManager<TPool, TItemKey, TSubType> : PoolManager<TPool, TItemKey>
        where TPool : class, IKeyProvider<string>
        where TSubType : PoolManager<TPool, TItemKey>
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
        }

        public bool RemoveManager (Guid libraryUid)
        {
            if (_default == libraryUid)
                _default = Guid.Empty;
            return _managers.Remove(libraryUid);
        }

        public Guid Default
        {
            get { return _default; }
            set
            {
                if (!_managers.ContainsKey(value))
                    throw new ArgumentException("Can only set default library UID to a value that has been previously added.");
            }
        }

        public override NamedObservableCollection<TPool> Pools
        {
            get { return _managers[MapAndCheckUid(_default)].Pools; }
        }

        public override TPool CreatePool (string name)
        {
            return _managers[MapAndCheckUid(_default)].CreatePool(name);
        }

        protected override TPool CreatePoolCore (string name)
        {
            throw new NotImplementedException();
        }

        public override void Reset ()
        {
            _managers[MapAndCheckUid(_default)].Reset();
        }

        public override TPool PoolFromItemKey (TItemKey key)
        {
            return _managers[MapAndCheckUid(_default)].PoolFromItemKey(key);
        }

        internal override TItemKey LastKey
        {
            get { return _managers[MapAndCheckUid(_default)].LastKey; }
            set { _managers[MapAndCheckUid(_default)].LastKey = value; }
        }

        internal override void LinkItemKey (TItemKey key, TPool pool)
        {
            _managers[MapAndCheckUid(_default)].LinkItemKey(key, pool);
        }

        internal override void UnlinkItemKey (TItemKey key)
        {
            _managers[MapAndCheckUid(_default)].UnlinkItemKey(key);
        }

        internal override TItemKey TakeKey ()
        {
            return _managers[MapAndCheckUid(_default)].TakeKey();
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
