using System;
using System.Collections.Generic;
using System.Text;
using Treefrog.Framework.Imaging;
using System.Xml.Serialization;
using Treefrog.Framework.Model.Proxy;

namespace Treefrog.Framework.Model
{
    public interface ITexturePool
    {
        int Count { get; }

        bool Contains (Guid uid);
        TextureResource GetResource (Guid uid);
        void AddResource (TextureResource resource);
        void RemoveResource (Guid uid);
        void Invalidate (Guid uid);

        event EventHandler<ResourceEventArgs> ResourceAdded;
        event EventHandler<ResourceEventArgs> ResourceRemoved;
        event EventHandler<ResourceEventArgs> ResourceInvalidated;
    }

    public class TexturePool : ITexturePool
    {
        private Dictionary<Guid, TextureResource> _resources;
        private Dictionary<Guid, int> _useCount;

        public TexturePool ()
        {
            _resources = new Dictionary<Guid, TextureResource>();
            _useCount = new Dictionary<Guid, int>();
        }

        public int Count
        {
            get { return _resources.Count; }
        }

        public bool Contains (Guid uid)
        {
            return _resources.ContainsKey(uid);
        }

        public TextureResource GetResource (Guid uid)
        {
            TextureResource resource;
            if (_resources.TryGetValue(uid, out resource))
                return resource;
            else
                return null;
        }

        public void AddResource (TextureResource resource)
        {
            if (resource == null)
                throw new ArgumentNullException("resource");

            if (_resources.ContainsKey(resource.Uid)) {
                _useCount[resource.Uid]++;
                return;
            }

            _resources.Add(resource.Uid, resource);
            _useCount.Add(resource.Uid, 1);

            OnResourceAdded(new ResourceEventArgs(resource.Uid));
        }

        public void RemoveResource (Guid uid)
        {
            if (_resources.ContainsKey(uid)) {
                _useCount[uid]--;
                if (_useCount[uid] > 0)
                    return;
            }

            if (_resources.Remove(uid)) {
                _useCount.Remove(uid);
                OnResourceRemoved(new ResourceEventArgs(uid));
            }
        }

        public void Invalidate (Guid uid)
        {
            OnResourceInvalidated(new ResourceEventArgs(uid));
        }

        public event EventHandler<ResourceEventArgs> ResourceAdded;
        public event EventHandler<ResourceEventArgs> ResourceRemoved;
        public event EventHandler<ResourceEventArgs> ResourceInvalidated;

        protected virtual void OnResourceAdded (ResourceEventArgs e)
        {
            EventHandler<ResourceEventArgs> ev = ResourceAdded;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnResourceRemoved (ResourceEventArgs e)
        {
            EventHandler<ResourceEventArgs> ev = ResourceRemoved;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnResourceInvalidated (ResourceEventArgs e)
        {
            EventHandler<ResourceEventArgs> ev = ResourceInvalidated;
            if (ev != null)
                ev(this, e);
        }

        public static LibraryX.TextureGroupX ToXmlProxyX (TexturePool pool)
        {
            if (pool == null)
                return null;

            List<LibraryX.TextureX> defs = new List<LibraryX.TextureX>();
            foreach (var kv in pool._resources)
                defs.Add(new LibraryX.TextureX() {
                    Uid = kv.Key,
                    TextureData = TextureResource.ToXmlProxy(kv.Value),
                });

            return new LibraryX.TextureGroupX() {
                Textures = defs,
            };
        }

        public static TexturePool FromXmlProxy (LibraryX.TextureGroupX proxy)
        {
            if (proxy == null)
                return null;

            TexturePool pool = new TexturePool();
            foreach (var defProxy in proxy.Textures) {
                TextureResource resource = TextureResource.FromXmlProxy(defProxy.TextureData, defProxy.Uid.ValueOrNew());
                if (resource != null) {
                    pool._resources[resource.Uid] = resource;
                    pool._useCount[resource.Uid] = 1;
                }
            }

            return pool;
        }
    }

    public class MetaTexturePool : ITexturePool
    {
        private Guid _default;
        private Dictionary<Guid, ITexturePool> _pools;

        public MetaTexturePool ()
        {
            _pools = new Dictionary<Guid, ITexturePool>();
        }

        public Guid Default
        {
            get { return _default; }
            set
            {
                if (!_pools.ContainsKey(value))
                    throw new ArgumentException("Can only set default library UID to a value that has been previously added.");
                _default = value;
            }
        }

        public ITexturePool GetPool (Guid libraryUid)
        {
            return _pools[MapAndCheckUid(libraryUid)];
        }

        public void AddPool (Guid libraryUid, ITexturePool pool)
        {
            if (libraryUid == Guid.Empty)
                throw new ArgumentException("Library UID must be non-empty.");
            if (_pools.ContainsKey(libraryUid))
                throw new ArgumentException("A pool with the given UID has already been added.");

            _pools.Add(libraryUid, pool);

            if (_pools.Count == 1)
                _default = libraryUid;

            //pool.PoolAdded += HandlePoolAdded;
            //pool.PoolRemoved += HandlePoolRemoved;
        }

        public bool RemovePool (Guid libraryUid)
        {
            if (_default == libraryUid)
                _default = Guid.Empty;

            ITexturePool pool;
            if (_pools.TryGetValue(libraryUid, out pool)) {
                //pool.PoolAdded -= HandlePoolAdded;
                //pool.PoolRemoved -= HandlePoolRemoved;
            }

            return _pools.Remove(libraryUid);
        }

        public int Count
        {
            get 
            {
                int accum = 0;
                foreach (var pool in _pools.Values)
                    accum += pool.Count;
                return accum;
            }
        }

        public bool Contains (Guid uid)
        {
            foreach (var pool in _pools.Values) {
                if (pool.Contains(uid))
                    return true;
            }
            return false;
        }

        public TextureResource GetResource (Guid uid)
        {
            foreach (var pool in _pools.Values) {
                TextureResource res = pool.GetResource(uid);
                if (res != null)
                    return res;
            }
            return null;
        }

        public void AddResource (TextureResource resource)
        {
            _pools[MapAndCheckUid(_default)].AddResource(resource);
        }

        public void RemoveResource (Guid uid)
        {
            foreach (var pool in _pools.Values)
                pool.RemoveResource(uid);
        }

        public void Invalidate (Guid uid)
        {
            foreach (var pool in _pools.Values)
                pool.Invalidate(uid);
        }

        public event EventHandler<ResourceEventArgs> ResourceAdded;

        public event EventHandler<ResourceEventArgs> ResourceRemoved;

        public event EventHandler<ResourceEventArgs> ResourceInvalidated;

        private void HandleResourceAdded (object sender, ResourceEventArgs e)
        {
            var ev = ResourceAdded;
            if (ev != null)
                ev(this, e);
        }

        private void HandleResourceRemoved (object sender, ResourceEventArgs e)
        {
            var ev = ResourceRemoved;
            if (ev != null)
                ev(this, e);
        }

        private void HandleResourceInvalidated (object sender, ResourceEventArgs e)
        {
            var ev = ResourceInvalidated;
            if (ev != null)
                ev(this, e);
        }

        private Guid MapAndCheckUid (Guid libraryUid)
        {
            if (libraryUid == Guid.Empty)
                libraryUid = _default;
            if (libraryUid == Guid.Empty)
                throw new InvalidOperationException("No default library has been set for this manager.");
            if (!_pools.ContainsKey(libraryUid))
                throw new ArgumentException("The specified library has not been registered with this manager.");

            return libraryUid;
        }
    }
}
