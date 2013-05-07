using System;
using System.Collections.Generic;
using System.Text;
using Treefrog.Framework.Imaging;
using System.Xml.Serialization;
using Treefrog.Framework.Model.Proxy;

namespace Treefrog.Framework.Model
{
    /*public class ResourceEventArgs : EventArgs
    {
        public Guid Uid { get; private set; }

        public ResourceEventArgs (Guid uid)
        {
            Uid = uid;
        }
    }*/

    public class TexturePool
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
}
