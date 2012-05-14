using System.Collections.Generic;
using System.Xml.Serialization;

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

    public class ObjectPoolManager : PoolManager<ObjectPool, int>
    {
        protected override ObjectPool CreatePoolCore (string name)
        {
            return new ObjectPool(name, this);
        }

        internal override int TakeKey ()
        {
            LastKey++;
            return LastKey;
        }

        public static ObjectPoolManagerXmlProxy ToXmlProxy (ObjectPoolManager manager)
        {
            if (manager == null)
                return null;

            List<ObjectPoolXmlProxy> pools = new List<ObjectPoolXmlProxy>();
            foreach (ObjectPool pool in manager.Pools)
                pools.Add(ObjectPool.ToXmlProxy(pool));

            return new ObjectPoolManagerXmlProxy()
            {
                LastKey = manager.LastKey,
                Pools = pools.ToArray(),
            };
        }

        public static ObjectPoolManager FromXmlProxy (ObjectPoolManagerXmlProxy proxy)
        {
            if (proxy == null)
                return null;

            ObjectPoolManager manager = new ObjectPoolManager();
            foreach (ObjectPoolXmlProxy pool in proxy.Pools)
                ObjectPool.FromXmlProxy(pool, manager);

            return manager;
        }
    }
}
