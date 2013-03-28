using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using Treefrog.Framework.Model.Proxy;

namespace Treefrog.Framework.Model
{
    public interface IObjectPoolManager : IPoolManager<ObjectPool>
    {
        TexturePool TexturePool { get; }
    }

    public class ObjectPoolManager : PoolManager<ObjectPool, ObjectClass>, IObjectPoolManager
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

        protected override void OnPoolAdded (ObjectPool pool)
        {
            if (pool.TexturePool != _texPool) {
                foreach (ObjectClass objClass in pool.Objects) {
                    if (!_texPool.Contains(objClass.Image.Uid))
                        _texPool.AddResource(objClass.Image);
                }

                pool.TexturePool = _texPool;
            }
        }

        public static LibraryX.ObjectGroupX ToXmlProxyX (ObjectPoolManager manager)
        {
            if (manager == null)
                return null;

            List<LibraryX.ObjectPoolX> pools = new List<LibraryX.ObjectPoolX>();
            foreach (ObjectPool pool in manager.Pools)
                pools.Add(ObjectPool.ToXProxy(pool));

            return new LibraryX.ObjectGroupX() {
                ObjectPools = pools,
            };
        }

        public static ObjectPoolManager FromXmlProxy (LibraryX.ObjectGroupX proxy, TexturePool texturePool)
        {
            if (proxy == null)
                return null;

            ObjectPoolManager manager = new ObjectPoolManager(texturePool);
            if (proxy.ObjectPools != null) {
                foreach (var pool in proxy.ObjectPools)
                    ObjectPool.FromXProxy(pool, manager);
            }

            return manager;
        }
    }

    public class MetaObjectPoolManager : MetaPoolManager<ObjectPool, ObjectClass, ObjectPoolManager>, IObjectPoolManager
    {
        public TexturePool TexturePool
        {
            get { return GetManager(Default).TexturePool; }
        }
    }
}
