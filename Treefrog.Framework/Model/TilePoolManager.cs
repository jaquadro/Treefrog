using System;
using System.Collections.Generic;
using System.Text;
using Treefrog.Framework.Imaging;
using System.Xml.Serialization;

namespace Treefrog.Framework.Model
{
    [XmlRoot("TilePools")]
    public class TilePoolManagerXmlProxy
    {
        [XmlAttribute]
        public int LastKey { get; set; }

        [XmlElement("TilePool")]
        public TilePoolXmlProxy[] Pools { get; set; }
    }

    public class TilePoolManager
    {
        private int _lastId = 0;
        private NamedResourceCollection<TilePool> _pools;
        private Dictionary<int, TilePool> _tileIndexMap;

        private TexturePool _texPool;

        public TilePoolManager (TexturePool texPool)
        {
            _texPool = texPool;

            _pools = new NamedResourceCollection<TilePool>();
            _pools.ResourceRemoved += PoolRemovedHandler;

            _tileIndexMap = new Dictionary<int, TilePool>();
        }

        private void PoolRemovedHandler (object sender, NamedResourceEventArgs<TilePool> e)
        {
            List<int> removeQueue = new List<int>();
            foreach (var item in _tileIndexMap) {
                if (item.Value == e.Resource)
                    removeQueue.Add(item.Key);
            }

            foreach (int key in removeQueue)
                _tileIndexMap.Remove(key);
        }

        public NamedResourceCollection<TilePool> Pools
        {
            get { return _pools; }
        }

        public TexturePool TexturePool
        {
            get { return _texPool; }
        }

        public TilePool CreateTilePool (string name, int tileWidth, int tileHeight)
        {
            if (_pools.Contains(name))
                throw new ArgumentException("Manager already contains a pool with the given name.", "name");

            TilePool pool = new TilePool(this, name, tileWidth, tileHeight);
            _pools.Add(pool);

            return pool;
        }

        public TilePool ImportTilePool (string name, TextureResource source, TilePool.TileImportOptions options)
        {
            if (_pools.Contains(name))
                throw new ArgumentException("Manager already contains a pool with the given name.", "name");

            TilePool pool = new TilePool(this, name, options.TileWidth, options.TileHeight);
            pool.ImportMerge(source, options);
            _pools.Add(pool);

            return pool;
        }

        public TilePool MergePool (string name, TilePool pool)
        {
            TilePool dst = null;
            if (_pools.Contains(name)) {
                dst = _pools[name];
                if (dst.TileWidth != pool.TileWidth || dst.TileHeight != pool.TileHeight)
                    throw new ArgumentException("Source pool tile dimensions do not match destination pool tile dimensions.");
            }
            else
                dst = CreateTilePool(name, pool.TileWidth, pool.TileHeight);

            foreach (Tile srcTile in pool) {
                dst.AddTile(pool.GetTileTexture(srcTile.Id));
            }

            return dst;
        }

        public void Reset ()
        {
            _lastId = 0;
            _pools.Clear();
            _tileIndexMap = new Dictionary<int, TilePool>();
        }

        public TilePool PoolFromTileId (int id)
        {
            TilePool pool = null;
            _tileIndexMap.TryGetValue(id, out pool);
            return pool;
        }

        internal int LastId
        {
            get { return _lastId; }
            set { _lastId = value; }
        }

        internal int TakeId ()
        {
            _lastId++;
            return _lastId;
        }

        internal void LinkTile (int id, TilePool pool)
        {
            _tileIndexMap[id] = pool;
        }

        internal void UnlinkTile (int id)
        {
            _tileIndexMap.Remove(id);
        }

        [Obsolete]
        public static TilePoolManagerXmlProxy ToXmlProxy (TilePoolManager manager)
        {
            if (manager == null)
                return null;

            List<TilePoolXmlProxy> pools = new List<TilePoolXmlProxy>();
            foreach (TilePool pool in manager.Pools)
                pools.Add(TilePool.ToXmlProxy(pool));

            return new TilePoolManagerXmlProxy()
            {
                LastKey = manager.LastId,
                Pools = pools.ToArray(),
            };
        }

        public static LibraryX.TileGroupX ToXmlProxyX (TilePoolManager manager)
        {
            if (manager == null)
                return null;

            List<LibraryX.TilePoolX> pools = new List<LibraryX.TilePoolX>();
            foreach (TilePool pool in manager.Pools)
                pools.Add(TilePool.ToXmlProxyX(pool));

            return new LibraryX.TileGroupX() {
                TilePools = pools,
            };
        }

        [Obsolete]
        public static TilePoolManager FromXmlProxy (TilePoolManagerXmlProxy proxy, TexturePool texturePool)
        {
            if (proxy == null)
                return null;

            TilePoolManager manager = new TilePoolManager(texturePool);

            if (proxy.Pools != null)
                foreach (TilePoolXmlProxy pool in proxy.Pools)
                    TilePool.FromXmlProxy(pool, manager);

            return manager;
        }

        public static TilePoolManager FromXmlProxy (LibraryX.TileGroupX proxy, TexturePool texturePool)
        {
            if (proxy == null)
                return null;

            TilePoolManager manager = new TilePoolManager(texturePool);

            if (proxy.TilePools != null)
                foreach (var pool in proxy.TilePools)
                    TilePool.FromXmlProxy(pool, manager);

            return manager;
        }
    }
}
