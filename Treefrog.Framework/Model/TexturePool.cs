using System;
using System.Collections.Generic;
using System.Text;
using Treefrog.Framework.Imaging;
using System.Xml.Serialization;

namespace Treefrog.Framework.Model
{
    public class ResourceEventArgs : EventArgs
    {
        public int Id { get; private set; }

        public ResourceEventArgs (int id)
        {
            Id = id;
        }
    }

    public class TexturePool
    {
        private int _lastId;
        private Dictionary<int, TextureResource> _resources;

        public TexturePool ()
        {
            _resources = new Dictionary<int, TextureResource>();
        }

        public int Count
        {
            get { return _resources.Count; }
        }

        public TextureResource GetResource (int id)
        {
            TextureResource resource;
            if (_resources.TryGetValue(id, out resource))
                return resource;
            else
                return null;
        }

        public int AddResource (TextureResource resource)
        {
            if (resource == null)
                throw new ArgumentNullException("resource");

            int id = ++_lastId;
            _resources.Add(id, resource);

            OnResourceAdded(new ResourceEventArgs(id));
            return id;
        }

        public void RemoveResource (int id)
        {
            if (_resources.Remove(id)) {
                if (id == _lastId)
                    _lastId--;
                OnResourceRemoved(new ResourceEventArgs(id));
            }
        }

        internal void ReplaceResource (int id, TextureResource resource)
        {
            if (GetResource(id) != resource) {
                _resources[id] = resource;
                Invalidate(id);
            }
        }

        public void Invalidate (int id)
        {
            OnResourceInvalidated(new ResourceEventArgs(id));
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

        public static TexturePoolXmlProxy ToXmlProxy (TexturePool pool)
        {
            if (pool == null)
                return null;

            List<TextureDefinitionXmlProxy> defs = new List<TextureDefinitionXmlProxy>();
            foreach (var kv in pool._resources)
                defs.Add(new TextureDefinitionXmlProxy() {
                    Id = kv.Key,
                    TextureData = TextureResource.ToXmlProxy(kv.Value),
                });

            return new TexturePoolXmlProxy() {
                Textures = defs,
            };
        }

        public static TexturePool FromXmlProxy (TexturePoolXmlProxy proxy)
        {
            if (proxy == null)
                return null;

            TexturePool pool = new TexturePool();
            foreach (TextureDefinitionXmlProxy defProxy in proxy.Textures) {
                TextureResource resource = TextureResource.FromXmlProxy(defProxy.TextureData);
                if (resource != null) {
                    pool._resources[defProxy.Id] = resource;
                    pool._lastId = Math.Max(pool._lastId, defProxy.Id);
                }
            }

            return pool;
        }
    }

    public class TexturePoolXmlProxy
    {
        [XmlArray]
        [XmlArrayItem("Texture")]
        public List<TextureDefinitionXmlProxy> Textures { get; set; }
    }

    public class TextureDefinitionXmlProxy
    {
        [XmlAttribute]
        public int Id { get; set; }

        [XmlElement]
        public TextureResource.XmlProxy TextureData { get; set; }
    }
}
