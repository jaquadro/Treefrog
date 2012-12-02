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

        public TilePoolManager ()
        {
            _pools = new NamedResourceCollection<TilePool>();
            _tileIndexMap = new Dictionary<int, TilePool>();
        }

        public NamedResourceCollection<TilePool> Pools
        {
            get { return _pools; }
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

        public void Reset ()
        {
            _lastId = 0;
            _pools.Clear();
            _tileIndexMap = new Dictionary<int, TilePool>();
        }

        public TilePool PoolFromTileId (int id)
        {
            return _tileIndexMap[id];
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

        public static TilePoolManager FromXmlProxy (TilePoolManagerXmlProxy proxy)
        {
            if (proxy == null)
                return null;

            TilePoolManager manager = new TilePoolManager();

            if (proxy.Pools != null)
                foreach (TilePoolXmlProxy pool in proxy.Pools)
                    TilePool.FromXmlProxy(pool, manager);

            return manager;
        }
    }
}
