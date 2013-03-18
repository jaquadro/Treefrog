using System;
using System.Collections.Generic;
using System.Text;
using Treefrog.Framework.Imaging;
using System.Xml.Serialization;
using Treefrog.Framework.Model.Proxy;

namespace Treefrog.Framework.Model
{
    public class ResourceEventArgs : EventArgs
    {
        public Guid Uid { get; private set; }

        public ResourceEventArgs (Guid uid)
        {
            Uid = uid;
        }
    }

    public class TexturePool
    {
        //private int _lastId;
        private Dictionary<Guid, TextureResource> _resources;

        public TexturePool ()
        {
            _resources = new Dictionary<Guid, TextureResource>();
        }

        public int Count
        {
            get { return _resources.Count; }
        }

        public TextureResource GetResource (Guid uid)
        {
            TextureResource resource;
            if (_resources.TryGetValue(uid, out resource))
                return resource;
            else
                return null;
        }

        public Guid AddResource (TextureResource resource)
        {
            if (resource == null)
                throw new ArgumentNullException("resource");

            Guid uid = Guid.NewGuid();
            _resources.Add(uid, resource);

            OnResourceAdded(new ResourceEventArgs(uid));
            return uid;
        }

        public void RemoveResource (Guid uid)
        {
            if (_resources.Remove(uid)) {
                OnResourceRemoved(new ResourceEventArgs(uid));
            }
        }

        internal void ReplaceResource (Guid uid, TextureResource resource)
        {
            if (GetResource(uid) != resource) {
                _resources[uid] = resource;
                Invalidate(uid);
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
                TextureResource resource = TextureResource.FromXmlProxy(defProxy.TextureData);
                if (resource != null) {
                    pool._resources[defProxy.Uid.ValueOrNew()] = resource;
                    //pool._lastId = Math.Max(pool._lastId, defProxy.Id);
                }
            }

            return pool;
        }
    }
}
