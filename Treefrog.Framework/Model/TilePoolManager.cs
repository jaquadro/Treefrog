using System;
using System.Collections.Generic;
using System.Text;
using Treefrog.Framework.Imaging;
using System.Xml.Serialization;
using Treefrog.Framework.Model.Proxy;

namespace Treefrog.Framework.Model
{
    public class TilePoolManager
    {
        private NamedResourceCollection<TilePool> _pools;
        private Dictionary<Guid, TilePool> _tileIndexMap;

        private TexturePool _texPool;

        public TilePoolManager (TexturePool texPool)
        {
            _texPool = texPool;

            _pools = new NamedResourceCollection<TilePool>();
            _pools.ResourceRemoved += PoolRemovedHandler;

            _tileIndexMap = new Dictionary<Guid, TilePool>();
        }

        private void PoolRemovedHandler (object sender, NamedResourceEventArgs<TilePool> e)
        {
            List<Guid> removeQueue = new List<Guid>();
            foreach (var item in _tileIndexMap) {
                if (item.Value == e.Resource)
                    removeQueue.Add(item.Key);
            }

            foreach (Guid key in removeQueue)
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
                dst.AddTile(pool.GetTileTexture(srcTile.Uid));
            }

            return dst;
        }

        public void Reset ()
        {
            _pools.Clear();
            _tileIndexMap = new Dictionary<Guid, TilePool>();
        }

        public TilePool PoolFromTileId (Guid uid)
        {
            TilePool pool = null;
            _tileIndexMap.TryGetValue(uid, out pool);
            return pool;
        }

        internal Guid TakeId ()
        {
            return Guid.NewGuid();
        }

        internal void LinkTile (Guid uid, TilePool pool)
        {
            _tileIndexMap[uid] = pool;
        }

        internal void UnlinkTile (Guid uid)
        {
            _tileIndexMap.Remove(uid);
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
