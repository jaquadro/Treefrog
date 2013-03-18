using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using Treefrog.Framework.Model.Proxy;

namespace Treefrog.Framework.Model
{
    [XmlRoot("ObjectPools")]
    public class ObjectPoolManagerXmlProxy
    {
        [XmlAttribute]
        public int LastKey { get; set; }

        [XmlElement("ObjectPool")]
        public ObjectPoolXmlProxy[] Pools { get; set; }
    }

    public class ObjectPoolManager : PoolManager<ObjectPool, Guid>
    {
        private TexturePool _texPool;

        public ObjectPoolManager (TexturePool texPool)
            : base()
        {
            _texPool = texPool;
        }

        public TexturePool TexturePool
        {
            get { return _texPool; }
        }

        protected override ObjectPool CreatePoolCore (string name)
        {
            return new ObjectPool(name, this);
        }

        internal override Guid TakeKey ()
        {
            return Guid.NewGuid();
        }

        [Obsolete]
        public static ObjectPoolManagerXmlProxy ToXmlProxy (ObjectPoolManager manager)
        {
            if (manager == null)
                return null;

            List<ObjectPoolXmlProxy> pools = new List<ObjectPoolXmlProxy>();
            foreach (ObjectPool pool in manager.Pools)
                pools.Add(ObjectPool.ToXmlProxy(pool));

            return new ObjectPoolManagerXmlProxy()
            {
                //LastKey = manager.LastKey,
                Pools = pools.ToArray(),
            };
        }

        public static LibraryX.ObjectGroupX ToXmlProxyX (ObjectPoolManager manager)
        {
            if (manager == null)
                return null;

            List<LibraryX.ObjectPoolX> pools = new List<LibraryX.ObjectPoolX>();
            foreach (ObjectPool pool in manager.Pools)
                pools.Add(ObjectPool.ToXmlProxyX(pool));

            return new LibraryX.ObjectGroupX() {
                ObjectPools = pools,
            };
        }

        [Obsolete]
        public static ObjectPoolManager FromXmlProxy (ObjectPoolManagerXmlProxy proxy, TexturePool texturePool)
        {
            if (proxy == null)
                return null;

            ObjectPoolManager manager = new ObjectPoolManager(texturePool);
            if (proxy.Pools != null) {
                foreach (ObjectPoolXmlProxy pool in proxy.Pools)
                    ObjectPool.FromXmlProxy(pool, manager);
            }

            return manager;
        }

        public static ObjectPoolManager FromXmlProxy (LibraryX.ObjectGroupX proxy, TexturePool texturePool)
        {
            if (proxy == null)
                return null;

            ObjectPoolManager manager = new ObjectPoolManager(texturePool);
            if (proxy.ObjectPools != null) {
                foreach (var pool in proxy.ObjectPools)
                    ObjectPool.FromXmlProxy(pool, manager);
            }

            return manager;
        }
    }
}
