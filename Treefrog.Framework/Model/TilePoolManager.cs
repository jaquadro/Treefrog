using System;
using System.Collections.Generic;
using System.Text;
using Treefrog.Framework.Imaging;
using System.Xml.Serialization;
using Treefrog.Framework.Model.Proxy;

namespace Treefrog.Framework.Model
{
    public interface ITilePoolManager : IPoolManager<TilePool, Guid>
    {
        TexturePool TexturePool { get; }
        TilePool CreatePool (string name, int tileWidth, int tileHeight);
        TilePool ImportPool (string name, TextureResource source, TilePool.TileImportOptions options);
        TilePool MergePool (string name, TilePool pool);
    }

    public class TilePoolManager : PoolManager<TilePool, Guid>, ITilePoolManager
    {
        public static int DefaultTileWidth = 16;
        public static int DefaultTileHeight = 16;

        private TexturePool _texPool;

        public TilePoolManager (TexturePool texPool)
        {
            _texPool = texPool;
        }

        public TexturePool TexturePool
        {
            get { return _texPool; }
        }

        protected override TilePool CreatePoolCore (string name)
        {
            return new TilePool(this, name, DefaultTileWidth, DefaultTileHeight);
        }

        public TilePool CreatePool (string name, int tileWidth, int tileHeight)
        {
            //if (Pools.Contains(name))
            //    throw new ArgumentException("Manager already contains a pool with the given name.", "name");

            TilePool pool = new TilePool(this, name, tileWidth, tileHeight);
            Pools.Add(pool);

            return pool;
        }

        public TilePool ImportPool (string name, TextureResource source, TilePool.TileImportOptions options)
        {
            //if (Pools.Contains(name))
            //    throw new ArgumentException("Manager already contains a pool with the given name.", "name");

            TilePool pool = new TilePool(this, name, options.TileWidth, options.TileHeight);
            pool.ImportMerge(source, options);
            Pools.Add(pool);

            return pool;
        }

        public TilePool MergePool (string name, TilePool pool)
        {
            TilePool dst = null;
            //if (Pools.Contains(name)) {
            //    dst = Pools[name];
            //    if (dst.TileWidth != pool.TileWidth || dst.TileHeight != pool.TileHeight)
            //        throw new ArgumentException("Source pool tile dimensions do not match destination pool tile dimensions.");
            //}
            //else
                dst = CreatePool(name, pool.TileWidth, pool.TileHeight);

            foreach (Tile srcTile in pool) {
                dst.AddTile(pool.GetTileTexture(srcTile.Uid));
            }

            return dst;
        }

        internal override Guid TakeKey ()
        {
            return Guid.NewGuid();
        }

        internal Guid TakeId ()
        {
            return Guid.NewGuid();
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

    public class MetaTilePoolManager : MetaPoolManager<TilePool, Guid, TilePoolManager>, ITilePoolManager
    {
        public TexturePool TexturePool
        {
            get { return GetManager(Default).TexturePool; }
        }

        public TilePool CreatePool (string name, int tileWidth, int tileHeight)
        {
            return GetManager(Default).CreatePool(name, tileWidth, tileHeight);
        }

        public TilePool ImportPool (string name, TextureResource source, TilePool.TileImportOptions options)
        {
            return GetManager(Default).ImportPool(name, source, options);
        }

        public TilePool MergePool (string name, TilePool pool)
        {
            return GetManager(Default).MergePool(name, pool);
        }
    }
}
